namespace BlazorWinbox;

public class WindowOptions : BasicWindowOptions
{
    //Callback methods:
    ///// <summary>
    ///// Callback triggered when the winbox element is being created. You can modify all these winbox options from this table passed as first parameter.
    ///// </summary>
    //public Action<WindowOptions> OnCreate { get; set; }
    /// <summary>
    /// Callback triggered when the window moves. with X and Y values
    /// </summary>
    public Func<string, string, Task> OnMove { get; set; }
    /// <summary>
    /// Callback triggered when the window resizes. with width and height
    /// </summary>
    public Func<string, string, Task> OnResize { get; set; }
    /// <summary>
    /// Callback triggered when the window enters fullscreen.
    /// </summary>
    public Func<bool?, Task> OnFullScreen { get; set; }
    /// <summary>
    /// Callback triggered when the window enters minimized mode.
    /// </summary>
    public Func<bool?, Task> OnMinimize { get; set; }
    /// <summary>
    /// Callback triggered when the window enters maximize mode.
    /// </summary>
    public Func<bool?, Task> OnMaximize { get; set; }
    /// <summary>
    /// Callback triggered when the window returns to a windowed state from a Fullscreen, Minimized or Maximized state.
    /// </summary>
    public Func<Task> OnRestore { get; set; }
    /// <summary>
    /// Callback triggered when the window is hidden with win.hide()
    /// </summary>
    public Func<bool?, Task> OnHide { get; set; }
    /// <summary>
    /// Callback triggered when the window is shown with win.show()
    /// </summary>
    public Func<bool?, Task> OnShow { get; set; }
    /// <summary>
    /// Callbacks triggered when the window is closing. The keyword this inside the callback function refers to the corresponding WinBox instance. Note: the event 'onclose' will be triggered right before closing and stops closing when a callback was applied and returns a truthy value.
    /// <para>takes force:bool parameter</para>
    /// <para>returns completeClose : bool</para>
    /// </summary>
    public Func<Task<bool>> OnClose { get; set; }

    /// <summary>
    /// Callbacks triggered when the window is closing. The keyword this inside the callback function refers to the corresponding WinBox instance. Note: the event 'onclose' will be triggered right before closing and stops closing when a callback was applied and returns a truthy value.
    /// <para>takes force:bool parameter</para>
    /// <para>returns completeClose : bool</para>
    /// </summary>
    public Func<Task<bool>> OnCloseAsync { get; set; }
    /// <summary>
    /// Callback triggered when a window goes into focused state. with force parameter
    /// </summary>
    public Func<bool?, Task> OnFocus { get; set; }
    /// <summary>
    /// Callback triggered when a window lost the focused state.
    /// </summary>
    public Func<bool?, Task> OnBlur { get; set; }
}
