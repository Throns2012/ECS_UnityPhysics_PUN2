using Unity.UIWidgets.animation;
using Unity.UIWidgets.widgets;

internal sealed class SimpleRouteBuilder
{
    public static PageRouteBuilder PageRouteBuilder(RouteSettings settings, WidgetBuilder builder)
        => new PageRouteBuilder(
            settings: settings,
            pageBuilder: new SimpleRouteBuilder(builder).Build
        );

    private SimpleRouteBuilder(WidgetBuilder builder)
    {
        _builder = builder;
    }

    private readonly WidgetBuilder _builder;

    private Widget Build(BuildContext context, Animation<float> animation, Animation<float> secondaryAnimation) => _builder(context);
}