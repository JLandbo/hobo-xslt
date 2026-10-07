namespace HoboXslt.Core;

// The window's normal (not maximized) size, and the pane widths as proportions of each other.
public sealed record Layout(double Width, double Height, double XmlWidth, double XsltWidth, double OutputWidth, double BottomHeight);