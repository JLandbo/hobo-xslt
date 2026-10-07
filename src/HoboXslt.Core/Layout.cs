namespace HoboXslt.Core;

// The window's normal (not maximized) size, and the panes while editing and while debugging.
public sealed record Layout(double Width, double Height, PaneLayout? Edit, PaneLayout? Debug)
{
    // A hand-edited settings file can hold sizes the window refuses, so such a layout is not used.
    public bool IsValid() => Width > 0 && Height > 0 && (Edit?.IsValid() ?? true) && (Debug?.IsValid() ?? true);
}

// The pane widths are proportions of each other; a hidden pane keeps the width it comes back with.
public sealed record PaneLayout(double XmlWidth, double XsltWidth, double OutputWidth, double BottomHeight, bool XmlHidden, bool OutputHidden)
{
    public bool IsValid() => XmlWidth >= 0 && XsltWidth >= 0 && OutputWidth >= 0 && BottomHeight >= 0;
}
