using Avalonia.Metadata;

namespace Avalonia.Controls.Platform;

[PrivateApi]
public interface IWaylandOptionsToplevelImplFeature
{
    bool IsLayerShellSupported { get; }
    void SetAppId(string? appId);
    void SetLayer(WaylandLayer? layer);
    void SetLayerAnchor(WaylandLayerAnchor anchor);
    void SetLayerExclusiveZone(int exclusiveZone);
    void SetLayerMargin(Thickness margin);
    void SetLayerKeyboardInteractivity(WaylandLayerKeyboardInteractivity interactivity);
    void SetLayerNamespace(string? layerNamespace);
    void SetLayerOutputName(string? outputName);
}
