using System;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using Avalonia.Reactive;

namespace Avalonia.Controls;

/// <summary>
/// Specifies the z-order layer for a Wayland layer-shell surface (<c>zwlr_layer_shell_v1.layer</c>).
/// </summary>
public enum WaylandLayer
{
    Background = 0,
    Bottom = 1,
    Top = 2,
    Overlay = 3,
}

/// <summary>
/// Edge anchors for a Wayland layer-shell surface (<c>zwlr_layer_surface_v1.anchor</c>).
/// </summary>
[Flags]
public enum WaylandLayerAnchor
{
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8,
    All = Top | Bottom | Left | Right,
}

/// <summary>
/// Keyboard focus policy for a Wayland layer-shell surface (<c>zwlr_layer_surface_v1.keyboard_interactivity</c>).
/// </summary>
public enum WaylandLayerKeyboardInteractivity
{
    None = 0,
    Exclusive = 1,
    OnDemand = 2,
}

/// <summary>
/// Set of Wayland specific properties and events that allow deeper customization of the application per platform.
/// </summary>
public static class WaylandProperties
{
    /// <summary>
    /// Defines the <c>AppId</c> attached property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specifies the application ID (<c>app_id</c>) sent to the Wayland compositor via <c>xdg_toplevel</c>
    /// for window management, application identification, and window grouping.
    /// </para>
    /// <para>
    /// In Wayland environments, client windows do not directly provide their own window icons. Instead,
    /// compositors, docks, and taskbars resolve the application icon, launcher identity, and notifications by
    /// matching the <c>AppId</c> against the filename of the application's installed <c>.desktop</c> file
    /// (for example, using <c>org.example.myapp</c> to match <c>org.example.myapp.desktop</c>).
    /// </para>
    /// </remarks>
    public static readonly AttachedProperty<string?> AppIdProperty =
        AvaloniaProperty.RegisterAttached<Window, string?>("AppId", typeof(WaylandProperties));

    /// <summary>
    /// Defines the <c>Layer</c> attached property to host a window as a <c>zwlr_layer_shell_v1</c> layer surface.
    /// </summary>
    public static readonly AttachedProperty<WaylandLayer?> LayerProperty =
        AvaloniaProperty.RegisterAttached<Window, WaylandLayer?>("Layer", typeof(WaylandProperties));

    /// <summary>
    /// Defines the <c>LayerAnchor</c> attached property for layer-shell surface edge anchoring.
    /// </summary>
    public static readonly AttachedProperty<WaylandLayerAnchor> LayerAnchorProperty =
        AvaloniaProperty.RegisterAttached<Window, WaylandLayerAnchor>("LayerAnchor", typeof(WaylandProperties));

    /// <summary>
    /// Defines the <c>LayerExclusiveZone</c> attached property (<c>-1</c> spans over panels/bars).
    /// </summary>
    public static readonly AttachedProperty<int> LayerExclusiveZoneProperty =
        AvaloniaProperty.RegisterAttached<Window, int>("LayerExclusiveZone", typeof(WaylandProperties));

    /// <summary>
    /// Defines the <c>LayerMargin</c> attached property for layer-shell edge margins.
    /// </summary>
    public static readonly AttachedProperty<Thickness> LayerMarginProperty =
        AvaloniaProperty.RegisterAttached<Window, Thickness>("LayerMargin", typeof(WaylandProperties));

    /// <summary>
    /// Defines the <c>LayerKeyboardInteractivity</c> attached property for layer-shell keyboard focus policy.
    /// </summary>
    public static readonly AttachedProperty<WaylandLayerKeyboardInteractivity> LayerKeyboardInteractivityProperty =
        AvaloniaProperty.RegisterAttached<Window, WaylandLayerKeyboardInteractivity>(
            "LayerKeyboardInteractivity",
            typeof(WaylandProperties),
            defaultValue: WaylandLayerKeyboardInteractivity.OnDemand);

    /// <summary>
    /// Defines the <c>LayerNamespace</c> attached property identifying the layer-shell surface namespace.
    /// </summary>
    public static readonly AttachedProperty<string?> LayerNamespaceProperty =
        AvaloniaProperty.RegisterAttached<Window, string?>("LayerNamespace", typeof(WaylandProperties));

    /// <summary>
    /// Defines the <c>LayerOutputName</c> attached property pinning the layer-shell surface to a named output.
    /// </summary>
    public static readonly AttachedProperty<string?> LayerOutputNameProperty =
        AvaloniaProperty.RegisterAttached<Window, string?>("LayerOutputName", typeof(WaylandProperties));

    /// <summary>
    /// Sets the value of the <c>AppId</c> attached property of the specified window.
    /// </summary>
    /// <param name="obj">The window.</param>
    /// <param name="value">The application ID value to set.</param>
    public static void SetAppId(Window obj, string? value) => obj.SetValue(AppIdProperty, value);

    /// <summary>
    /// Gets the value of the <c>AppId</c> attached property of the specified window.
    /// </summary>
    /// <param name="obj">The window.</param>
    /// <returns>The application ID for <paramref name="obj"/>.</returns>
    public static string? GetAppId(Window obj) => obj.GetValue(AppIdProperty);

    /// <summary>Sets the <c>Layer</c> attached property on <paramref name="obj"/>.</summary>
    public static void SetLayer(Window obj, WaylandLayer? value) => obj.SetValue(LayerProperty, value);

    /// <summary>Gets the <c>Layer</c> attached property on <paramref name="obj"/>.</summary>
    public static WaylandLayer? GetLayer(Window obj) => obj.GetValue(LayerProperty);

    /// <summary>Sets the <c>LayerAnchor</c> attached property on <paramref name="obj"/>.</summary>
    public static void SetLayerAnchor(Window obj, WaylandLayerAnchor value) => obj.SetValue(LayerAnchorProperty, value);

    /// <summary>Gets the <c>LayerAnchor</c> attached property on <paramref name="obj"/>.</summary>
    public static WaylandLayerAnchor GetLayerAnchor(Window obj) => obj.GetValue(LayerAnchorProperty);

    /// <summary>Sets the <c>LayerExclusiveZone</c> attached property on <paramref name="obj"/>.</summary>
    public static void SetLayerExclusiveZone(Window obj, int value) => obj.SetValue(LayerExclusiveZoneProperty, value);

    /// <summary>Gets the <c>LayerExclusiveZone</c> attached property on <paramref name="obj"/>.</summary>
    public static int GetLayerExclusiveZone(Window obj) => obj.GetValue(LayerExclusiveZoneProperty);

    /// <summary>Sets the <c>LayerMargin</c> attached property on <paramref name="obj"/>.</summary>
    public static void SetLayerMargin(Window obj, Thickness value) => obj.SetValue(LayerMarginProperty, value);

    /// <summary>Gets the <c>LayerMargin</c> attached property on <paramref name="obj"/>.</summary>
    public static Thickness GetLayerMargin(Window obj) => obj.GetValue(LayerMarginProperty);

    /// <summary>Sets the <c>LayerKeyboardInteractivity</c> attached property on <paramref name="obj"/>.</summary>
    public static void SetLayerKeyboardInteractivity(Window obj, WaylandLayerKeyboardInteractivity value) =>
        obj.SetValue(LayerKeyboardInteractivityProperty, value);

    /// <summary>Gets the <c>LayerKeyboardInteractivity</c> attached property on <paramref name="obj"/>.</summary>
    public static WaylandLayerKeyboardInteractivity GetLayerKeyboardInteractivity(Window obj) =>
        obj.GetValue(LayerKeyboardInteractivityProperty);

    /// <summary>Sets the <c>LayerNamespace</c> attached property on <paramref name="obj"/>.</summary>
    public static void SetLayerNamespace(Window obj, string? value) => obj.SetValue(LayerNamespaceProperty, value);

    /// <summary>Gets the <c>LayerNamespace</c> attached property on <paramref name="obj"/>.</summary>
    public static string? GetLayerNamespace(Window obj) => obj.GetValue(LayerNamespaceProperty);

    /// <summary>Sets the <c>LayerOutputName</c> attached property on <paramref name="obj"/>.</summary>
    public static void SetLayerOutputName(Window obj, string? value) => obj.SetValue(LayerOutputNameProperty, value);

    /// <summary>Gets the <c>LayerOutputName</c> attached property on <paramref name="obj"/>.</summary>
    public static string? GetLayerOutputName(Window obj) => obj.GetValue(LayerOutputNameProperty);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="topLevel"/> is hosted on Avalonia's native Wayland backend.
    /// </summary>
    public static bool IsWaylandBackend(TopLevel topLevel) =>
        topLevel?.PlatformImpl?.TryGetFeature<IWaylandOptionsToplevelImplFeature>() != null;

    /// <summary>
    /// Returns <c>true</c> if <paramref name="topLevel"/> runs on native Wayland and the compositor supports <c>zwlr_layer_shell_v1</c>.
    /// </summary>
    public static bool IsLayerShellSupported(TopLevel topLevel) =>
        topLevel?.PlatformImpl?.TryGetFeature<IWaylandOptionsToplevelImplFeature>()?.IsLayerShellSupported == true;

    static WaylandProperties()
    {
        AppIdProperty.Changed.Subscribe(OnAppIdChanged);
        LayerProperty.Changed.Subscribe(OnLayerChanged);
        LayerAnchorProperty.Changed.Subscribe(OnLayerAnchorChanged);
        LayerExclusiveZoneProperty.Changed.Subscribe(OnLayerExclusiveZoneChanged);
        LayerMarginProperty.Changed.Subscribe(OnLayerMarginChanged);
        LayerKeyboardInteractivityProperty.Changed.Subscribe(OnLayerKeyboardInteractivityChanged);
        LayerNamespaceProperty.Changed.Subscribe(OnLayerNamespaceChanged);
        LayerOutputNameProperty.Changed.Subscribe(OnLayerOutputNameChanged);
    }

    private static IWaylandOptionsToplevelImplFeature? TryGetFeature(AvaloniaPropertyChangedEventArgs e)
        => (e.Sender as TopLevel)?.PlatformImpl?.TryGetFeature<IWaylandOptionsToplevelImplFeature>();

    private static void OnAppIdChanged(AvaloniaPropertyChangedEventArgs<string?> e) =>
        TryGetFeature(e)?.SetAppId(e.NewValue.GetValueOrDefault(null));

    private static void OnLayerChanged(AvaloniaPropertyChangedEventArgs<WaylandLayer?> e) =>
        TryGetFeature(e)?.SetLayer(e.NewValue.GetValueOrDefault());

    private static void OnLayerAnchorChanged(AvaloniaPropertyChangedEventArgs<WaylandLayerAnchor> e) =>
        TryGetFeature(e)?.SetLayerAnchor(e.NewValue.GetValueOrDefault());

    private static void OnLayerExclusiveZoneChanged(AvaloniaPropertyChangedEventArgs<int> e) =>
        TryGetFeature(e)?.SetLayerExclusiveZone(e.NewValue.GetValueOrDefault());

    private static void OnLayerMarginChanged(AvaloniaPropertyChangedEventArgs<Thickness> e) =>
        TryGetFeature(e)?.SetLayerMargin(e.NewValue.GetValueOrDefault());

    private static void OnLayerKeyboardInteractivityChanged(AvaloniaPropertyChangedEventArgs<WaylandLayerKeyboardInteractivity> e) =>
        TryGetFeature(e)?.SetLayerKeyboardInteractivity(e.NewValue.GetValueOrDefault());

    private static void OnLayerNamespaceChanged(AvaloniaPropertyChangedEventArgs<string?> e) =>
        TryGetFeature(e)?.SetLayerNamespace(e.NewValue.GetValueOrDefault(null));

    private static void OnLayerOutputNameChanged(AvaloniaPropertyChangedEventArgs<string?> e) =>
        TryGetFeature(e)?.SetLayerOutputName(e.NewValue.GetValueOrDefault(null));
}
