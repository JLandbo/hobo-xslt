namespace HoboXslt.Core;

// The window's normal (not maximized) size, and the panes while editing and while debugging.
public sealed record Layout(double Width, double Height, PaneLayout? Edit, PaneLayout? Debug);

// The pane widths are proportions of each other; a hidden pane keeps the width it comes back with.
public sealed record PaneLayout(double XmlWidth, double XsltWidth, double OutputWidth, double BottomHeight, bool XmlHidden, bool OutputHidden);
