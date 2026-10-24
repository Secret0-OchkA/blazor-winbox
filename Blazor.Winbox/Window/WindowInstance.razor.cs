using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Reflection;

namespace BlazorWinbox;

public partial class WindowInstance : IDisposable
{
    [Inject] public IWindowManager WindowManager { get; set; }
    [Parameter] public Guid Id { get; set; }
    [Parameter] public WindowOptions Options { get; set; }
    //[Parameter] public string Title { get; set; }
    [Parameter] public RenderFragment Content { get; set; }
    [CascadingParameter] public GlobalWindowOptions GlobalWindowOptions { get; set; }

    WindowOptions _Options;
    private DotNetObjectReference<WindowInstance> _InstanceReference;
    private bool _IsDisposed;

    /// <summary>
    /// Reference that lets javascript call back into this instance.
    /// </summary>
    internal DotNetObjectReference<WindowInstance> InstanceReference => _InstanceReference;

    protected override void OnInitialized()
    {
        ConfigureInstance();
    }

    /// <summary>
    /// Releases the reference that keeps this component alive on the javascript side,
    /// otherwise every closed window stays in memory forever.
    /// </summary>
    public void Dispose()
    {
        if (_IsDisposed)
        {
            return;
        }
        _IsDisposed = true;

        _InstanceReference?.Dispose();
        _InstanceReference = null;
    }

    private void ConfigureInstance()
    {
        Options ??= new WindowOptions();

        SetClass();
        SetIndex();
        SetRoot();
        SetBackground();
        SetBorder();
        SetModal();
        SetWidth();
        SetHeight();
        SetMinWidth();
        SetMinHeight();
        SetMaxWidth();
        SetMaxHeight();
        SetAutoSize();
        SetX();
        SetY();
        SetTop();
        SetBottom();
        SetRight();
        SetLeft();
        SetDocking();
        SetToolbar();
        SetControlButtons();
        Options.Id = Id.ToString("N");
        _InstanceReference = DotNetObjectReference.Create(this);

        Options.OnMoveHandlerName = nameof(OnMove);
        Options.OnShowHandlerName = nameof(OnShow);
        Options.OnHideHandlerName = nameof(OnHide);
        Options.OnFocusHandlerName = nameof(OnFocus);
        Options.OnBlurHandlerName = nameof(OnBlur);
        Options.OnResizeHandlerName = nameof(OnResize);
        Options.OnFullScreenHandlerName = nameof(OnFullscreen);
        Options.OnMaximizeHandlerName = nameof(OnMaximize);
        Options.OnMinimizeHandlerName = nameof(OnMinimize);
        Options.OnRestoreHandlerName = nameof(OnRestore);

        _Options = Options;
    }


    #region Configure instance methods

    private void SetClass()
    {
        Options.Class = (GlobalWindowOptions.Class ?? "") + (Options.Class ?? "");
    }

    private void SetIndex()
    {
        if (Options.Index.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.Index.HasValue)
        {
            Options.Index = GlobalWindowOptions.Index.Value;
        }
    }

    private void SetRoot()
    {
        if (Options.Root.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.Root.HasValue)
        {
            Options.Root = GlobalWindowOptions.Root.Value;
        }
    }

    private void SetBackground()
    {
        if (!string.IsNullOrEmpty(Options.Background))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Background))
        {
            Options.Background = GlobalWindowOptions.Background;
        }
    }

    private void SetBorder()
    {
        if (!string.IsNullOrEmpty(Options.Border))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Border))
        {
            Options.Border = GlobalWindowOptions.Border;
        }
    }

    private void SetModal()
    {
        if (Options.Modal.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.Modal.HasValue)
        {
            Options.Modal = GlobalWindowOptions.Modal.Value;
        }
        else
        {
            Options.Modal = false;
        }
    }

    private void SetWidth()
    {
        if (!string.IsNullOrEmpty(Options.Width))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Width))
        {
            Options.Width = GlobalWindowOptions.Width;
        }
    }

    private void SetHeight()
    {
        if (!string.IsNullOrEmpty(Options.Height))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Height))
        {
            Options.Height = GlobalWindowOptions.Height;
        }
    }

    private void SetMinWidth()
    {
        if (!string.IsNullOrEmpty(Options.MinWidth))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MinWidth))
        {
            Options.MinWidth = GlobalWindowOptions.MinWidth;
        }
    }

    private void SetMinHeight()
    {
        if (!string.IsNullOrEmpty(Options.MinHeight))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MinHeight))
        {
            Options.MinHeight = GlobalWindowOptions.MinHeight;
        }
    }

    private void SetMaxWidth()
    {
        if (!string.IsNullOrEmpty(Options.MaxWidth))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MaxWidth))
        {
            Options.MaxWidth = GlobalWindowOptions.MaxWidth;
        }
    }

    private void SetMaxHeight()
    {
        if (!string.IsNullOrEmpty(Options.MaxHeight))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MaxHeight))
        {
            Options.MaxHeight = GlobalWindowOptions.MaxHeight;
        }
    }

    private void SetDocking()
    {
        if (Options.Docking is not null)
        {
            return;
        }
        else if (GlobalWindowOptions.Docking is not null)
        {
            Options.Docking = GlobalWindowOptions.Docking;
        }
    }

    private void SetToolbar()
    {
        List<ToolbarButton> xGlobalButtons = GlobalWindowOptions?.Toolbar;
        List<ToolbarButton> xInstanceButtons = Options.Toolbar;

        if (xGlobalButtons is null or { Count: 0 } && xInstanceButtons is null or { Count: 0 })
        {
            return;
        }

        List<ToolbarButton> xButtons = new();

        if (xGlobalButtons is not null && !ReferenceEquals(xGlobalButtons, xInstanceButtons))
        {
            xButtons.AddRange(xGlobalButtons.Where(x => x is not null));
        }

        if (xInstanceButtons is not null)
        {
            foreach (ToolbarButton iButton in xInstanceButtons.Where(x => x is not null))
            {
                int xIndex = string.IsNullOrEmpty(iButton.Id)
                    ? -1
                    : xButtons.FindIndex(x => x.Id == iButton.Id);

                if (xIndex >= 0)
                {
                    xButtons[xIndex] = iButton;
                }
                else
                {
                    xButtons.Add(iButton);
                }
            }
        }

        foreach (ToolbarButton iButton in xButtons)
        {
            if (string.IsNullOrEmpty(iButton.Id))
            {
                iButton.Id = Guid.NewGuid().ToString("N");
            }
        }

        Options.Toolbar = xButtons;
    }

    private void SetControlButtons()
    {
        Options.Minimize = MergeControlButton(Options.Minimize, GlobalWindowOptions?.Minimize);
        Options.Maximize = MergeControlButton(Options.Maximize, GlobalWindowOptions?.Maximize);
        Options.Fullscreen = MergeControlButton(Options.Fullscreen, GlobalWindowOptions?.Fullscreen);
        Options.Close = MergeControlButton(Options.Close, GlobalWindowOptions?.Close);
    }

    private static readonly PropertyInfo[] ControlButtonProperties = typeof(WindowControlButton).GetProperties();

    /// <summary>
    /// Creates the effective control settings: values of the window level object win over the global ones.
    /// </summary>
    private static WindowControlButton MergeControlButton(WindowControlButton instanceButton, WindowControlButton globalButton)
    {
        if (instanceButton is null && globalButton is null)
        {
            return null;
        }

        WindowControlButton xMerged = new();
        bool xAnyValueSet = false;

        foreach (PropertyInfo iProperty in ControlButtonProperties)
        {
            object xValue = instanceButton is null ? null : iProperty.GetValue(instanceButton);

            if (IsUnsetValue(xValue))
            {
                xValue = globalButton is null ? null : iProperty.GetValue(globalButton);
            }

            if (!IsUnsetValue(xValue))
            {
                xAnyValueSet = true;
            }

            iProperty.SetValue(xMerged, xValue);
        }

        return xAnyValueSet ? xMerged : null;
    }

    private static bool IsUnsetValue(object value)
    {
        return value is null || value is string xString && string.IsNullOrEmpty(xString);
    }

    private void SetAutoSize()
    {
        if (Options.AutoSize.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.AutoSize.HasValue)
        {
            Options.AutoSize = GlobalWindowOptions.AutoSize.Value;
        }
    }

    private void SetX()
    {
        if (!string.IsNullOrEmpty(Options.X))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.X))
        {
            Options.X = GlobalWindowOptions.X;
        }
        else
        {
            Options.X = "center";
        }
    }

    private void SetY()
    {
        if (!string.IsNullOrEmpty(Options.Y))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Y))
        {
            Options.Y = GlobalWindowOptions.Y;
        }
        else
        {
            Options.Y = "center";
        }
    }

    private void SetTop()
    {
        if (!string.IsNullOrEmpty(Options.Top))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Top))
        {
            Options.Top = GlobalWindowOptions.Top;
        }
    }

    private void SetBottom()
    {
        if (!string.IsNullOrEmpty(Options.Bottom))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Bottom))
        {
            Options.Bottom = GlobalWindowOptions.Bottom;
        }
    }

    private void SetLeft()
    {
        if (!string.IsNullOrEmpty(Options.Left))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Left))
        {
            Options.Left = GlobalWindowOptions.Left;
        }
    }

    private void SetRight()
    {
        if (!string.IsNullOrEmpty(Options.Right))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Right))
        {
            Options.Right = GlobalWindowOptions.Right;
        }
    }

    #endregion

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JSInvokable]
    public void OnXClose()
    {
        WindowManager.CloseBlzr(Id, WindowResult.Cancel());
    }

    public async Task CloseAsync(WindowResult result)
    {
        bool? xCompleteClose = null;
        if (_Options.OnClose is not null)
            xCompleteClose = await _Options.OnClose.Invoke();

        bool? xCompleteCloseAsync = null;
        if (_Options.OnCloseAsync is not null)
            xCompleteCloseAsync = await _Options.OnCloseAsync?.Invoke();

        if (xCompleteClose is not false && xCompleteCloseAsync is not false)
            ForceClose(result);
    }

    [JSInvokable]
    public async Task CloseAsync()
    {
        await CloseAsync(WindowResult.Cancel());
    }

    public void ForceClose()
    {
        ForceClose(WindowResult.Cancel());
    }

    public void ForceClose(WindowResult result)
    {
        WindowManager.Close(Id, result);
    }

    /// <summary>
    /// Set the fullscreen state of a window. The window is expanded to the viewport while the title bar and toolbar stay visible.
    /// <para>The <see cref="WindowOptions.OnFullScreen"/> callback is triggered by the window itself once the state changed.</para>
    /// </summary>
    /// <param name="state">true to enter fullscreen, false to leave it, null to toggle it</param>
    public Task Fullscreen(bool? state = null)
    {
        WindowManager.Fullscreen(Id, state);
        return Task.CompletedTask;
    }
    [JSInvokable]
    public async Task OnFullscreen(bool? state = null)
    {
        if (_Options.OnFullScreen is null) return;
        await _Options.OnFullScreen.Invoke(state);
    }

    /// <summary>
    /// Hide a specific window
    /// </summary>
    /// <param name="state"></param>
    public async Task Hide(bool? state = null)
    {
        WindowManager.Hide(Id, state);
        if (_Options.OnHide is null) return;
        await _Options.OnHide.Invoke(state);
    }
    [JSInvokable]
    public async Task OnHide(bool? state = null)
    {
        if (_Options.OnHide is null) return;
        await _Options.OnHide.Invoke(state);
    }

    /// <summary>
    /// Set the maximized state of a window
    /// </summary>
    /// <param name="state"></param>
    public async Task Maximize(bool? state = null)
    {
        WindowManager.Maximize(Id, state);
        if (_Options.OnMaximize is null) return;
        await _Options.OnMaximize.Invoke(state);
    }
    [JSInvokable]
    public async Task OnMaximize(bool? state = null)
    {
        if (_Options.OnMaximize is null) return;
        await _Options.OnMaximize.Invoke(state);
    }

    /// <summary>
    /// Set the minimized state of a window
    /// </summary>
    /// <param name="state"></param>
    public async Task Minimize(bool? state = null)
    {
        WindowManager.Minimize(Id, state);
        if (_Options.OnMinimize is null) return;
        await _Options.OnMinimize.Invoke(state);
    }
    [JSInvokable]
    public async Task OnMinimize(bool? state = null)
    {
        if (_Options.OnMinimize is null) return;
        await _Options.OnMinimize.Invoke(state);
    }

    /// <summary>
    /// Supports keywords "right" for x-axis, "bottom" for y-axis, "center" for both, units px and % also for both.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public async Task Move(string x, string y)
    {
        WindowManager.Move(Id, x, y);
        if (_Options.OnMove is null) return;
        await _Options.OnMove.Invoke(x, y);
    }

    [JSInvokable]
    public async Task OnMove(long x, long y)
    {
        if (_Options.OnMove is null) return;
        await _Options.OnMove.Invoke(x.ToString(), y.ToString());
    }

    /// <summary>
    /// You can remove all control classes from above along the window's lifetime
    /// </summary>
    /// <param name="cssClass"></param>
    public void RemoveClass(string cssClass)
    {
        WindowManager.RemoveClass(Id, cssClass);
    }

    /// <summary>
    /// You can add all control classes from above along the window's lifetime
    /// </summary>
    /// <param name="cssClass"></param>
    public void AddClass(string cssClass)
    {
        WindowManager.AddClass(Id, cssClass);
    }

    /// <summary>
    /// Supports units px and % also for both.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public async Task Resize(string width, string height)
    {
        WindowManager.Resize(Id, width, height);
        if (_Options.OnResize is null) return;
        await _Options.OnResize.Invoke(width, height);
    }
    [JSInvokable]
    public async Task OnResize(long w, long h)
    {
        if (_Options.OnResize is null) return;
        await _Options.OnResize.Invoke(w.ToString(), h.ToString());
    }

    /// <summary>
    /// Restore the state of a window
    /// </summary>
    public async Task Restore()
    {
        WindowManager.Restore(Id);
        if (_Options.OnRestore is null) return;
        await _Options.OnRestore.Invoke();
    }
    [JSInvokable]
    public async Task OnRestore()
    {
        if (_Options.OnRestore is null) return;
        await _Options.OnRestore.Invoke();
    }

    public void SetBackground(string backgroundColor)
    {
        WindowManager.SetBackground(Id, backgroundColor);
    }

    public void SetTitle(string title)
    {
        WindowManager.SetTitle(Id, title);
    }

    /// <summary>
    /// Show a specific hidden window
    /// </summary>
    /// <param name="state"></param>
    public async Task Show(bool? state = null)
    {
        WindowManager.Show(Id, state);
        if (_Options.OnShow is null) return;
        await _Options.OnShow.Invoke(state);
    }
    [JSInvokable]
    public async Task OnShow(bool? state = null)
    {
        if (_Options.OnShow is null) return;
        await _Options.OnShow.Invoke(state);
    }

    /// <summary>
    /// You can toggle all control classes from above along the window's lifetime
    /// </summary>
    /// <param name="cssClass"></param>
    public void ToggleClass(string cssClass)
    {
        WindowManager.ToggleClass(Id, cssClass);
    }

    /// <summary>
    /// Focus a window (bring up to front)
    /// </summary>
    /// <param name="state"></param>
    public async Task Focus(bool? state = null)
    {
        WindowManager.Focus(Id);
        if (_Options.OnFocus is null) return;
        await _Options.OnFocus.Invoke(state);
    }
    [JSInvokable]
    public async Task OnFocus(bool? state = null)
    {
        if (_Options.OnFocus is null) return;
        await _Options.OnFocus.Invoke(state);
    }

    /// <summary>
    /// Blur a focused window
    /// </summary>
    /// <param name="state"></param>
    public async Task Blur(bool? state = null)
    {
        WindowManager.Blur(Id, state);
        if (_Options.OnBlur is null) return;
        await _Options.OnBlur.Invoke(state);
    }
    [JSInvokable]
    public async Task OnBlur(bool? state = null)
    {
        if (_Options.OnBlur is null) return;
        await _Options.OnBlur.Invoke(state);
    }

    /// <summary>
    /// Mount an HTML element to the window body.
    /// </summary>
    /// <param name="windowContent">HTML element (widget, template, etc.)</param>
    public void Mount(ElementReference windowContent)
    {
        WindowManager.Mount(Id, windowContent);
    }

    /// <summary>
    /// Adds a custom button to the window toolbar.
    /// <para>If a button with the same <see cref="ToolbarButton.Id"/> already exists it is replaced.</para>
    /// </summary>
    /// <param name="button">Button to add. A new <see cref="ToolbarButton.Id"/> is generated when it is not set.</param>
    public void AddToolbarButton(ToolbarButton button)
    {
        ArgumentNullException.ThrowIfNull(button);

        if (string.IsNullOrEmpty(button.Id))
        {
            button.Id = Guid.NewGuid().ToString("N");
        }

        _Options.Toolbar ??= new List<ToolbarButton>();

        int xIndex = _Options.Toolbar.FindIndex(x => x.Id == button.Id);
        if (xIndex >= 0)
        {
            _Options.Toolbar[xIndex] = button;
        }
        else
        {
            _Options.Toolbar.Add(button);
        }

        WindowManager.AddToolbarButton(Id, button);
    }

    /// <summary>
    /// Updates an existing toolbar button (icon, tooltip, colors, visibility, disabled state) or adds it when it does not exist yet.
    /// <para>The existing click action is kept when <see cref="ToolbarButton.OnClick"/> is not set on the passed button.</para>
    /// </summary>
    /// <param name="button">Button with the <see cref="ToolbarButton.Id"/> of the button to update.</param>
    public void UpdateToolbarButton(ToolbarButton button)
    {
        ArgumentNullException.ThrowIfNull(button);

        if (string.IsNullOrEmpty(button.Id))
        {
            throw new ArgumentException($"{nameof(ToolbarButton)}.{nameof(ToolbarButton.Id)} is required to update a toolbar button.", nameof(button));
        }

        _Options.Toolbar ??= new List<ToolbarButton>();

        int xIndex = _Options.Toolbar.FindIndex(x => x.Id == button.Id);
        if (xIndex >= 0)
        {
            button.OnClick ??= _Options.Toolbar[xIndex].OnClick;
            _Options.Toolbar[xIndex] = button;
        }
        else
        {
            _Options.Toolbar.Add(button);
        }

        WindowManager.UpdateToolbarButton(Id, button);
    }

    /// <summary>
    /// Removes a custom button from the window toolbar.
    /// </summary>
    /// <param name="buttonId"><see cref="ToolbarButton.Id"/> of the button to remove.</param>
    public void RemoveToolbarButton(string buttonId)
    {
        if (string.IsNullOrEmpty(buttonId))
        {
            return;
        }

        _Options.Toolbar?.RemoveAll(x => x.Id == buttonId);
        WindowManager.RemoveToolbarButton(Id, buttonId);
    }

    /// <summary>
    /// Replaces all custom buttons of the window toolbar.
    /// </summary>
    /// <param name="buttons">New toolbar buttons. A new <see cref="ToolbarButton.Id"/> is generated for buttons which have no id.</param>
    public void SetToolbarButtons(IEnumerable<ToolbarButton> buttons)
    {
        List<ToolbarButton> xButtons = buttons?.Where(x => x is not null).ToList() ?? new List<ToolbarButton>();

        foreach (ToolbarButton iButton in xButtons)
        {
            if (string.IsNullOrEmpty(iButton.Id))
            {
                iButton.Id = Guid.NewGuid().ToString("N");
            }
        }

        _Options.Toolbar = xButtons;
        WindowManager.SetToolbarButtons(Id, xButtons);
    }

    /// <summary>
    /// Customizes a built-in window control (icon, tooltip, colors, visibility, disabled state) after the window was opened.
    /// <para>Values which are not set on the passed object are taken from the global options.
    /// Passing null restores the default look of the control.</para>
    /// </summary>
    /// <param name="kind">Built-in control to customize.</param>
    /// <param name="button">New settings of the control.</param>
    public void UpdateControlButton(WindowControlKind kind, WindowControlButton button)
    {
        WindowControlButton xEffective = MergeControlButton(button, GetGlobalControlButton(kind));

        switch (kind)
        {
            case WindowControlKind.Minimize:
                _Options.Minimize = xEffective;
                break;
            case WindowControlKind.Maximize:
                _Options.Maximize = xEffective;
                break;
            case WindowControlKind.Fullscreen:
                _Options.Fullscreen = xEffective;
                break;
            case WindowControlKind.Close:
                _Options.Close = xEffective;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown window control.");
        }

        WindowManager.UpdateControlButton(Id, kind, xEffective);
    }

    private WindowControlButton GetGlobalControlButton(WindowControlKind kind)
    {
        return kind switch
        {
            WindowControlKind.Minimize => GlobalWindowOptions?.Minimize,
            WindowControlKind.Maximize => GlobalWindowOptions?.Maximize,
            WindowControlKind.Fullscreen => GlobalWindowOptions?.Fullscreen,
            WindowControlKind.Close => GlobalWindowOptions?.Close,
            _ => null
        };
    }

    /// <summary>
    /// Invoked from javascript when a custom toolbar button is clicked.
    /// </summary>
    [JSInvokable]
    public async Task OnToolbarButtonClick(string buttonId)
    {
        ToolbarButton xButton = _Options.Toolbar?.FirstOrDefault(x => x.Id == buttonId);

        if (xButton?.OnClick is not null)
        {
            await xButton.OnClick.Invoke();
        }
    }
}
