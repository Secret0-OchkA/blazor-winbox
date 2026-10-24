# Blazor Winbox

Multiple Windows Library that wraps [WinBox.js](https://github.com/nextapps-de/winbox) in Blazor Components.

*Windows management and rendering processes were inspired  by [MudBlazor](https://mudblazor.com) dialogs*
<br />
<br />

## Compatibility
Only Blazor WASM (client-side) .NET 6 applications are currently supported. This library is not tested on Blazor Server applications.
<br />
<br />

## Getting Started
### Installation:
The Nuget package page can be found [here](https://www.nuget.org/packages/Blazor.Winbox/)

To install `Blazor.Winbox` from Nuget Gallery simply search for its name and install the latest version

<br />

##### Or using Package Manager run the following command
```bash
Install-Package Blazor.Winbox
```
##### Or using .NET CLI run the following command
```bash
dotnet add package Blazor.Winbox
```

### Configurations:
* Import `BlazorWinbox` by adding the following in `_Imports.razor`
```
@using BlazorWinbox
```

* Register Service in `Program` Class:
 ```c#
    using BlazorWinbox; 


    builder.Services.AddBlazorWinbox();
 ```
 * Add script references to `index.html` after Blazor script
  ```html
  <script src="_content/Blazor.Winbox/winbox.bundle.min.js"></script>
  <script src="_content/Blazor.Winbox/BlazorWinbox.min.js"></script>
  ```
  * Add the following component to your `MainLayout.razor` 
  ```html
<WindowProvider />
  ```

### Open a window
 Create a `window` component. Let's use `Counter` template as example:
```html
<Window>

    <h1>Counter</h1>

    <p role="status">Current count: @currentCount</p>

    <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

</Window>

```
```c#
@code {

    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```
**Note that whole window content should surrounded with `<Window>` Tag**

<br/>

To open window inject `IWindowManager` service and use `Open<TComponent>()` method:

```c#
//Use in razor
@inject IWindowManager windowManager
//OR in C# 
[Inject] public IWindowManager WindowManager { get; set; }

//Open a window
void OpenCounter(){
    IWindowReference windowReference = WindowManager.Open<Counter>();
}
```

![Preview](./Docs/BlazorWinbox.gif)

### Docking windows
Windows can dock (snap) to each other while they are dragged or resized. When the edge of the moved window gets close to the edge of another window, the window snaps to it and a dashed guide line shows the docking edge.

Docking is disabled by default. Enable it for a single window by assigning `WindowDockingOptions` to `WindowOptions.Docking`:

```c#
WindowOptions xWindowOptions = new()
{
    Width = "400px",
    Height = "300px",
    Docking = new WindowDockingOptions()
};

IWindowReference xWindowReference = WindowManager.Open<Counter>("Counter", windowOptions: xWindowOptions);
```

To enable docking for every window, assign it to the global options of `WindowProvider`:

```c#
_GlobalWindowOptions = new GlobalWindowOptions
{
    Docking = new WindowDockingOptions()
};
```

`WindowDockingOptions` properties:

| Property | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Enables the docking behavior when the options object is assigned |
| `SnapDistance` | `10` | Max distance (px) between window edges at which the window snaps |
| `ShowGuides` | `true` | Shows dashed guide lines at the docked edges |
| `GuideColor` | light blue | Any css color of the guide lines, e.g. `"#22c55e"` or `"rgba(34, 197, 94, .8)"` |
| `GuideThickness` | `1` | Thickness of the guide lines in pixels |
| `Group` | `null` | Windows dock only to windows of the same group, so unrelated window sets can be kept apart |

Docking group example — blue and green windows dock only inside their own group:

```c#
WindowOptions xBlueOptions = new() { Docking = new WindowDockingOptions { Group = "blue" } };
WindowOptions xGreenOptions = new()
{
    Docking = new WindowDockingOptions { Group = "green", GuideColor = "#22c55e", GuideThickness = 2 }
};
```

If you have the `WinBox` instance at hand (e.g. from custom JavaScript), docking can also be changed after the window was created:

```js
const win = WinBoxWindowManager.OpenWindow(title, windowOptions); // or your own WinBox instance
WinBoxDocking.attach(win, { snapDistance: 20, group: "blue" });   // (re)enable with new settings
WinBoxDocking.detach(win);                                        // disable
```

### Fullscreen
Calling `WindowInstance.Fullscreen()`, `IWindowManager.Fullscreen(id, state)` or clicking the built-in fullscreen button puts the window into the browser fullscreen (the Fullscreen API), so the browser toolbar disappears and the window fills the whole screen. The title bar and the toolbar of the window stay visible and the window content is not re-mounted, so it keeps working exactly as in the windowed state.

```c#
await windowInstance.Fullscreen(true);   // enter
await windowInstance.Fullscreen(false);  // leave
await windowInstance.Fullscreen();       // toggle
```

* The previous position and size are restored when fullscreen is left.
* Leaving fullscreen with `Esc` or by another browser action is synchronized back to Blazor — the window returns to its previous bounds and `OnRestore` is triggered.
* When the browser refuses the native request (a programmatic call without a user gesture, an iframe without the `allow="fullscreen"` permission, ...) the window is expanded to the viewport instead and the `Top`/`Bottom`/`Left`/`Right` limits are respected.
* `Maximize`, `Minimize`, `Restore` and `Close` leave fullscreen first.
* `OnFullScreen` is triggered once when the window enters fullscreen, `OnRestore` — once when it returns to the windowed state.

### Custom toolbar buttons
Besides the built-in minimize, maximize, fullscreen and close buttons, a window can display custom title bar buttons with their own icons and click actions.

Assign a list of `ToolbarButton` to `WindowOptions.Toolbar` to add buttons to a single window, or to the global options of `WindowProvider` to add them to every window (instance buttons are merged with the global ones, a button with the same `Id` overrides the global button).

```c#
WindowOptions xWindowOptions = new()
{
    Width = "480px",
    Height = "320px",
    Toolbar = new List<ToolbarButton>
    {
        new()
        {
            Id = "refresh",
            Tooltip = "Refresh data",
            IconClass = "oi oi-loop-circular",   // icon font / css classes
            OnClick = async () => await LoadDataAsync()
        },
        new()
        {
            Tooltip = "Company logo",            // icon image (svg, png, data uri, ...)
            Icon = "./logo.png",
            IconSize = "18px 18px",
            OnClick = () => { OpenSite(); return Task.CompletedTask; }
        },
        new()
        {
            Tooltip = "Unavailable action",
            IconClass = "oi oi-ban",
            Disabled = true
        }
    }
};

WindowManager.Open<MyWindow>("My window", windowOptions: xWindowOptions);
```

Built-in buttons are customizable in the same way — icon, tooltip, colors, extra css, visibility and disabled state. Their built-in behavior (minimize, maximize, fullscreen, close) stays the same:

```c#
WindowOptions xWindowOptions = new()
{
    Minimize = new WindowControlButton { Tooltip = "Collapse", Icon = "./icons/minimize.svg" },
    Maximize = new WindowControlButton { IconClass = "oi oi-resize-both", Color = "#bae6fd" },
    Fullscreen = new WindowControlButton { Visible = false },   // hide the control
    Close = new WindowControlButton { Tooltip = "Close", Disabled = true }
};
```

`WindowControlButton` supports `Tooltip`, `Icon`, `IconClass`, `IconSize`, `CssClass`, `Background`, `Color`, `Style`, `Visible` and `Disabled`. The same objects can be assigned to the global options of `WindowProvider`; window level values override the global ones property by property.

To hide all built-in controls and keep custom buttons only, set `Visible = false` for each of them:

```c#
WindowOptions xWindowOptions = new()
{
    Minimize = new WindowControlButton { Visible = false },
    Maximize = new WindowControlButton { Visible = false },
    Fullscreen = new WindowControlButton { Visible = false },
    Close = new WindowControlButton { Visible = false },
    Toolbar = new List<ToolbarButton>
    {
        new() { Tooltip = "Close", IconClass = "oi oi-x", OnClick = () => CloseWindowAsync() }
    }
};
```

Controls can also be hidden with the WinBox control classes (`Class = "no-min no-max no-full no-close"`), but keep in mind that a hidden close button can only be replaced by your own custom button which closes the window (e.g. `WindowInstance.CloseAsync()` or `IWindowManager.Close(id, WindowResult.Cancel())`).

`ToolbarButton` properties:

| Property | Description |
| --- | --- |
| `Id` | Unique button id. Generated automatically when not set. Used to update or remove the button later |
| `Tooltip` | Tooltip text (also used as `aria-label`) |
| `Icon` | Icon image url rendered as the button background |
| `IconClass` | Css classes of an icon font element rendered inside the button, e.g. `"oi oi-plus"` |
| `IconSize` | Css size of `Icon`, default is `16px 16px` |
| `CssClass` | Extra css classes of the button element |
| `Background` / `Color` | Background and foreground (icon) colors, any css color |
| `Style` | Raw css declarations, e.g. `"border-radius: 4px;"` |
| `Visible` | Hides the button when `false`. Default is `true` |
| `Disabled` | Ignores clicks when `true`. Default is `false` |
| `Index` | Zero-based position inside the controls container (`0` is the leftmost position next to the title). When not set, buttons are placed in declaration order starting from the leftmost position |
| `OnClick` | Action invoked when the button is clicked |

Buttons can also be changed after the window was opened. Get the `WindowInstance` from the window content (cascading parameter) or from the `Window` component reference:

```c#
// replace all buttons
WindowInstance.SetToolbarButtons(buttons);

// add a button, returns a generated Id on the button object
WindowInstance.AddToolbarButton(new ToolbarButton
{
    Tooltip = "Pin",
    IconClass = "oi oi-pin",
    OnClick = () => { Pin(); return Task.CompletedTask; }
});

// change an existing button (its click action is kept when OnClick is null)
WindowInstance.UpdateToolbarButton(new ToolbarButton
{
    Id = "pin",
    Tooltip = "Unpin",
    IconClass = "oi oi-pin",
    Disabled = true
});

// remove a button by id
WindowInstance.RemoveToolbarButton("pin");

// customize a built-in control at runtime (null restores the default look)
WindowInstance.UpdateControlButton(WindowControlKind.Close, new WindowControlButton
{
    Tooltip = "Close is disabled",
    Disabled = true
});
```

The same operations are exposed by `IWindowManager`: `AddToolbarButton`, `UpdateToolbarButton`, `RemoveToolbarButton`, `SetToolbarButtons` and `UpdateControlButton` take the window id as the first argument.

