using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using FontStyle = Unity.UIWidgets.ui.FontStyle;

namespace UIWidgetsSample
{
    public class UIWidgetsExample : UIWidgetsPanel
    {
        protected override Widget createWidget()
        {
            return new WidgetsApp(
                home: new ExampleApp(),
                pageRouteBuilder: (settings, builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (context, _, __) => builder(context)
                    )
            );
        }

        class ExampleApp : StatefulWidget
        {
            public ExampleApp(Key key = null) : base(key)
            {
            }

            public override State createState()
            {
                return new ExampleState();
            }
        }

        class ExampleState : State<ExampleApp>
        {
            int counter = 0;

            public override Widget build(BuildContext context)
            {
                return new Column(
                    children: new List<Widget> {
                         new Text("Counter: " + this.counter),
                         new GestureDetector(
                             onTap: () => {
                                 this.setState(()
                                     => {
                                     this.counter++;
                                 });
                             },
                             child: new Container(
                                 padding: EdgeInsets.symmetric(20, 20),
                                 color: Colors.blue,
                                 child: new Text("Click Me")
                             )
                         )
                    }
                );
            }
        }
    }
}