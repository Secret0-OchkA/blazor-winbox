using Microsoft.AspNetCore.Components;

namespace BlazorWinbox;

public interface IWindowManager
{
    event Action<IWindowReference> OnWindowInstanceAdded;
    event Action<IWindowReference, WindowResult> OnWindowCloseRequested;

    /// <summary>
    /// Open a window of any type
    /// </summary>
    /// <typeparam name="TComponent">Component type(must be blazor component)</typeparam>
    /// <param name="title">Optional title to appear on window title bar</param>
    /// <param name="windowParameters">Parameters that component has
    /// <para>should be a Key Value Pairs of parameters names and values</para>
    /// </param>
    /// <param name="windowOptions">Options of window</param>
    /// <returns>a window reference</returns>
    IWindowReference Open<TComponent>(string title = null, WindowParameters windowParameters = null, WindowOptions windowOptions = null) where TComponent : ComponentBase;
    IWindowReference Open(Type componentType, string title = null, WindowParameters windowParameters = null, WindowOptions windowOptions = null);

    /// <summary>
    /// Open a window of any type and get the result data returned by the window
    /// </summary>
    /// <typeparam name="TComponent">Component type(must be blazor component)</typeparam>
    /// <param name="title">Optional title to appear on window title bar</param>
    /// <param name="windowParameters">Parameters that component has
    /// <para>should be a Key Value Pairs of parameters names and values</para>
    /// </param>
    /// <param name="windowOptions">Options of window</param>
    /// <returns>data of the <see cref="WindowResult"/> returned by the window on close</returns>
    Task<object> OpenReValAsync<TComponent>(string title = null, WindowParameters windowParameters = null, WindowOptions windowOptions = null) where TComponent : ComponentBase;
    Task InjectComponentIntoWindow(string title, WindowOptions options);
    bool HasJsWindowReference(Guid id);
    void CloseBlzr(Guid winId, WindowResult result);
    public void Close(Guid winId, WindowResult result);
    /// <summary>
    /// Expands the window to the viewport while the title bar and toolbar stay visible.
    /// </summary>
    /// <param name="winId">Id of the window</param>
    /// <param name="state">true to enter fullscreen, false to leave it, null to toggle it</param>
    void Fullscreen(Guid winId, bool? state = null);
    void Hide(Guid winId, bool? state = null);
    void Maximize(Guid winId, bool? state = null);
    void Minimize(Guid winId, bool? state = null);
    void Move(Guid winId, string x, string y);
    void RemoveClass(Guid winId, string cssClass);
    void Resize(Guid winId, string width, string height);
    void Restore(Guid winId);
    void SetBackground(Guid winId, string backgroundColor);
    void SetTitle(Guid winId, string title);
    void Show(Guid winId, bool? state = null);
    void ToggleClass(Guid winId, string cssClass);
    void Focus(Guid winId, bool? state = null);
    void Blur(Guid winId, bool? state = null);
    void Mount(Guid winId, ElementReference windowContent);
    void AddClass(Guid id, string cssClass);

    /// <summary>
    /// Adds a custom button to the window toolbar, replacing the button with the same id when it exists.
    /// </summary>
    /// <param name="winId">Id of the window</param>
    /// <param name="button">Button to add</param>
    void AddToolbarButton(Guid winId, ToolbarButton button);

    /// <summary>
    /// Updates a custom button of the window toolbar, adding it when it does not exist yet.
    /// </summary>
    /// <param name="winId">Id of the window</param>
    /// <param name="button">Button with the id of the button to update</param>
    void UpdateToolbarButton(Guid winId, ToolbarButton button);

    /// <summary>
    /// Removes a custom button from the window toolbar.
    /// </summary>
    /// <param name="winId">Id of the window</param>
    /// <param name="buttonId">Id of the button to remove</param>
    void RemoveToolbarButton(Guid winId, string buttonId);

    /// <summary>
    /// Replaces all custom buttons of the window toolbar.
    /// </summary>
    /// <param name="winId">Id of the window</param>
    /// <param name="buttons">New toolbar buttons</param>
    void SetToolbarButtons(Guid winId, IEnumerable<ToolbarButton> buttons);

    /// <summary>
    /// Customizes one of the built-in window controls (minimize, maximize, fullscreen, close).
    /// </summary>
    /// <param name="winId">Id of the window</param>
    /// <param name="kind">Built-in control to customize</param>
    /// <param name="button">New settings of the control, null restores the default look</param>
    void UpdateControlButton(Guid winId, WindowControlKind kind, WindowControlButton button);
}
