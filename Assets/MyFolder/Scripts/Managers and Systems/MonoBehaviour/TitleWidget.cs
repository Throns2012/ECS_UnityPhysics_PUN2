using System;
using System.Collections.Generic;
using Assets.MyFolder.Scripts.Managers_and_Systems;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UIWidgetsSample;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Assets.MyFolder.Scripts
{
    public sealed class TitleWidget : UIWidgetsPanel, IMatchmakingCallbacks, IInRoomCallbacks, INetworkConnector
    {
        protected override Widget createWidget()
        {
            return new WidgetsApp(
                initialRoute: "/",
                textStyle: new TextStyle(fontSize: 24),
                pageRouteBuilder: SimpleRouteBuilder.PageRouteBuilder,
                routes: new Dictionary<string, WidgetBuilder> {
                    {"/", context => new RootScreen(this)},
                    {"/lobby-failed", context => new LobbyFailedScreen() },
                    {"/lobby-waiting", context => new LobbyWaitingScreen(this)},
                    {"/lobby-success", context => new LobbySuccessScreen()},
                }
            );
        }

        public sealed class LobbyFailedScreen : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new NavigationPage(title: "Sorry! Failed Connecting,");
            }
        }

        sealed class RootScreen : StatelessWidget
        {
            private INetworkConnector _connector;
            public RootScreen(INetworkConnector connector) => _connector = connector;
            public override Widget build(BuildContext context)
            {
                return new NavigationPage(
                    body: new Container(
                        color: Color.white,
                            child: new Center(
                                child: new CustomButton(
                                    onPressed: () => { Navigator.pushNamed(context, _connector.Connect() ? "/lobby-waiting" : "/lobby-failed"); },
                                    child: new Text("Connect to Server")
                                )
                            )
                        ),
                    title: "Title"
                );
            }
        }

        sealed class LobbyWaitingScreen : StatelessWidget, IConnectionCallbacks, IDisposable
        {
            private readonly INetworkConnector _connector;
            private BuildContext _context;
            public LobbyWaitingScreen(INetworkConnector connector)
            {
                _connector = connector;
                PhotonNetwork.AddCallbackTarget(this);
            }

            public override Widget build(BuildContext context)
            {
                _context = context;
                return new NavigationPage(
                    body: new Container(
                        color: new Color(0xFF1389FD),
                        child: new Center(
                            child: new Column(
                                children: new List<Widget>() {
                                    new CustomButton(onPressed: () =>
                                    {
                                        _connector.Disconnect();
                                    }, child: new Text("Back")),
                                }
                            )
                        )),
                    title: "Detail");
            }

            public void OnConnected()
            {
                Debug.Log("FEJOHBFIUEBIF");
            }

            public void OnConnectedToMaster()
            {
                Debug.Log("\\\\\\");
                Navigator.popAndPushNamed(_context, "/lobby-success");
            }

            public void OnDisconnected(DisconnectCause cause)
            {
                Debug.Log("381371969");
                Navigator.pop(_context);
            }

            public void OnRegionListReceived(RegionHandler regionHandler) { }

            public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }

            public void OnCustomAuthenticationFailed(string debugMessage) { }

            public void Dispose()
            {
                PhotonNetwork.RemoveCallbackTarget(this);
            }
        }

        sealed class LobbySuccessScreen : StatelessWidget
        {
            public override Widget build(BuildContext context)
            {
                return new NavigationPage(
                    body: new Container(
                        color: new Color(0xFF1389FD),
                        child: new Center(
                            child: new Column(
                                children: new List<Widget>() {
                                    new CustomButton(onPressed: () => { Navigator.pop(context); }, child: new Text("Back")),
                                }
                            )
                        )),
                    title: "Server Connection Success");
            }
        }

        public class CustomButton : StatelessWidget
        {
            public CustomButton(
                Key key = null,
                GestureTapCallback onPressed = null,
                EdgeInsets padding = null,
                Color backgroundColor = null,
                Widget child = null
            ) : base(key: key)
            {
                this.onPressed = onPressed;
                this.padding = padding ?? EdgeInsets.all(8.0f);
                this.backgroundColor = backgroundColor ?? CLColors.transparent;
                this.child = child;
            }

            public readonly GestureTapCallback onPressed;
            public readonly EdgeInsets padding;
            public readonly Widget child;
            public readonly Color backgroundColor;

            public override Widget build(BuildContext context)
            {
                return new GestureDetector(
                    onTap: this.onPressed,
                    child: new Container(
                        padding: this.padding,
                        color: this.backgroundColor,
                        child: this.child
                    )
                );
            }
        }

        sealed class NavigationPage : StatelessWidget
        {
            private readonly Widget _body;
            private readonly string _title;

            public NavigationPage(Widget body = null, string title = null)
            {
                _title = title;
                _body = body;
            }

            public override Widget build(BuildContext context)
            {
                Widget back = null;
                if (Navigator.of(context).canPop())
                {
                    back = new CustomButton(onPressed: () => { Navigator.pop(context); },
                        child: new Text("Go Back"));
                    back = new Column(mainAxisAlignment: MainAxisAlignment.center, children: new List<Widget>() { back });
                }


                return new Container(
                    child: new Column(
                        children: new List<Widget>
                        {
                            new ConstrainedBox(
                                constraints: new BoxConstraints(maxHeight: 80),
                                child: new DecoratedBox(
                                    decoration: new BoxDecoration(color: new Color(0XFFE1ECF4)),
                                    child: new NavigationToolbar(
                                        leading: back,
                                        middle: new Text(this._title, textAlign: TextAlign.center)
                                    )
                                )
                            ),
                            new Flexible(child: _body)
                        }
                    )
                );
            }
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            if (Application.isEditor) return;
#endif
            DontDestroyOnLoad(transform.parent);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            FontManager.instance.addFont(Resources.Load<Font>("NotoMono-Regular"), "noto");
#if UNITY_EDITOR
            if (!Application.isEditor)
#endif
                PhotonNetwork.AddCallbackTarget(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList) { }

        public void OnCreatedRoom() { }

        public void OnCreateRoomFailed(short returnCode, string message) { }

        public void OnJoinedRoom() { }

        public void OnJoinRoomFailed(short returnCode, string message) { }

        public void OnJoinRandomFailed(short returnCode, string message) { }

        public void OnLeftRoom() { }

        public void OnPlayerEnteredRoom(Player newPlayer) { }

        public void OnPlayerLeftRoom(Player otherPlayer) { }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }

        public void OnMasterClientSwitched(Player newMasterClient) { }

        public bool Connect()
        {
            Debug.Log("MFOIEHN");
            return PhotonNetwork.ConnectUsingSettings();
        }

        public void Disconnect()
        {
            PhotonNetwork.Disconnect();
        }
    }
}