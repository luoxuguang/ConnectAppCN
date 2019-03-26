using System;
using System.Collections.Generic;
using ConnectApp.api;
using ConnectApp.components;
using ConnectApp.components.refresh;
using ConnectApp.constants;
using ConnectApp.models;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;
using EventType = ConnectApp.models.EventType;

namespace ConnectApp.screens {
    public class EventsScreen : StatefulWidget {
        public EventsScreen(Key key = null) : base(key) {
        }

        public override State createState() {
            return new _EventsScreenState();
        }
    }

    internal class _EventsScreenState : State<EventsScreen> {
        private const float headerHeight = 80;
        private PageController _pageController;
        private int _selectedIndex;
        private int pageNumber = 1;
        private int completedPageNumber = 1;
        private float _offsetY = 0;

        public override void initState() {
            base.initState();
            if (StoreProvider.store.state.eventState.ongoingEvents.Count == 0) {
                StoreProvider.store.Dispatch(new FetchEventsAction {pageNumber = 1, tab = "ongoing"});
                StoreProvider.store.Dispatch(new FetchEventsAction {pageNumber = 1, tab = "completed"});
            }

            _pageController = new PageController();
            _selectedIndex = 0;
        }


        private bool _onNotification(ScrollNotification notification) {
            var pixels = notification.metrics.pixels;
            if (pixels >= 0) {
                if (pixels <= headerHeight) setState(() => { _offsetY = pixels / 2; });
            }
            else {
                if (_offsetY != 0) setState(() => { _offsetY = 0; });
            }

            return true;
        }


        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        new CustomNavigationBar(
                            new Text("活动", style: CTextStyle.H2),
                            null,
                            CColors.White,
                            _offsetY
                        ),
                        _buildSelectView(context),
                        _buildContentView(context)
                    }
                )
            );
        }

        private Widget _buildSelectItem(string title, int index) {
            Widget lineView = new Positioned(new Container());
            if (index == _selectedIndex)
                lineView = new Positioned(
                    bottom: 0,
                    left: 0,
                    right: 0,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget> {
                            new Container(
                                width: 80,
                                height: 2,
                                decoration: new BoxDecoration(
                                    CColors.PrimaryBlue
                                )
                            )
                        }
                    )
                );

            return new Container(
                child: new Stack(
                    children: new List<Widget> {
                        new CustomButton(
                            onPressed: () => {
                                if (_selectedIndex != index) setState(() => _selectedIndex = index);
                                _pageController.animateToPage(
                                    index,
                                    new TimeSpan(0, 0,
                                        0, 0, 250),
                                    Curves.ease
                                );
                            },
                            child: new Container(
                                height: 44,
                                width: 96,
                                alignment: Alignment.center,
                                child: new Text(
                                    title,
                                    style: index == _selectedIndex ? CTextStyle.PLargeBlue : CTextStyle.PLargeMedium
                                )
                            )
                        ),
                        lineView
                    }
                )
            );
        }

        private Widget _buildSelectView(BuildContext context) {
            return new Container(
                child: new Container(
                    decoration: new BoxDecoration(
                        border: new Border(bottom: new BorderSide(CColors.Separator2))
                    ),
                    height: 44,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.start,
                        children: new List<Widget> {
                            _buildSelectItem("即将开始", 0), 
                            _buildSelectItem("往期活动", 1)
                        }
                    )
                )
            );
        }

        private Widget _buildOngoingEventList(BuildContext context) {
            return new Container(
                child: new StoreConnector<AppState, Dictionary<string, object>>(
                    converter: (state, dispatch) => new Dictionary<string, object> {
                        {"loading", state.eventState.eventsLoading},
                        {"ongoingEvents", state.eventState.ongoingEvents},
                        {"ongoingEventDict", state.eventState.ongoingEventDict},
                        {"ongoingEventTotal", state.eventState.ongoingEventTotal}
                    },
                    builder: (context1, viewModel) => {
                        var loading = (bool) viewModel["loading"];
                        if (loading) return new GlobalLoading();
                        
                        var ongoingEvents = viewModel["ongoingEvents"] as List<string>;
                        var ongoingEventDict = viewModel["ongoingEventDict"] as Dictionary<string, IEvent>;
                        var ongoingEventTotal = (int) viewModel["ongoingEventTotal"];

                        RefresherCallback onFooterCallback = null;
                        if (ongoingEvents.Count < ongoingEventTotal) {
                            onFooterCallback = onFooterRefresh;
                        }
                        return new Refresh(
                            onHeaderRefresh: onHeaderRefresh,
                            onFooterRefresh: onFooterCallback,
                            headerBuilder: (cxt, controller) => new RefreshHeader(controller),
                            footerBuilder: (cxt, controller) => new RefreshFooter(controller),
                            child: ListView.builder(
                                physics: new AlwaysScrollableScrollPhysics(),
                                itemCount: ongoingEvents.Count,
                                itemBuilder: (cxt, index) => {
                                    var eventId = ongoingEvents[index];
                                    var model = ongoingEventDict[eventId];
                                    return new EventCard(
                                        model,
                                        () => {
                                            StoreProvider.store.Dispatch(new NavigatorToEventDetailAction {
                                                eventId = model.id,
                                                eventType = model.mode == "online" ? EventType.onLine : EventType.offline
                                            });
                                            Navigator.pushNamed(context, "/event-detail");
                                        },
                                        new ObjectKey(model.id)
                                    );
                                }
                            )
                        );
                    }
                )
            );
        }

        private Widget _buildCompletedEventList(BuildContext context) {
            return new Container(
                child: new StoreConnector<AppState, Dictionary<string, object>>(
                    converter: (state, dispatch) => new Dictionary<string, object> {
                        {"loading", state.eventState.eventsLoading},
                        {"completedEvents", state.eventState.completedEvents},
                        {"completedEventDict", state.eventState.completedEventDict},
                        {"completedEventTotal", state.eventState.completedEventTotal}
                    },
                    builder: (context1, viewModel) => {
                        var loading = (bool) viewModel["loading"];
                        if (loading) return new GlobalLoading();
                        
                        var completedEvents = viewModel["completedEvents"] as List<string>;
                        var completedEventDict = viewModel["completedEventDict"] as Dictionary<string, IEvent>;
                        var completedEventTotal = (int) viewModel["completedEventTotal"];

                        RefresherCallback onFooterCallback = null;
                        if (completedEvents.Count < completedEventTotal) {
                            onFooterCallback = onFooterRefresh;
                        }
                        
                        return new Refresh(
                            onHeaderRefresh: onHeaderRefresh,
                            onFooterRefresh: onFooterCallback,
                            headerBuilder: (cxt, controller) => new RefreshHeader(controller),
                            footerBuilder: (cxt, controller) => new RefreshFooter(controller),
                            child: ListView.builder(
                                physics: new AlwaysScrollableScrollPhysics(),
                                itemCount: completedEvents.Count,
                                itemBuilder: (cxt, index) => {
                                    var eventId = completedEvents[index];
                                    var model = completedEventDict[eventId];
                                    return new EventCard(
                                        model,
                                        () => {
                                            StoreProvider.store.Dispatch(new NavigatorToEventDetailAction {
                                                eventId = model.id,
                                                eventType = model.mode == "online" ? EventType.onLine: EventType.offline
                                            });
                                            Navigator.pushNamed(context, "/event-detail");
                                        }
                                    );
                                }
                            )
                        );
                    }
                )
            );
        }

        private Widget _buildContentView(BuildContext context) {
            return new Flexible(
                child: new Container(
                    padding: EdgeInsets.only(bottom: 49),
                    child: new PageView(
                        physics: new BouncingScrollPhysics(),
                        controller: _pageController,
                        onPageChanged: index => { setState(() => { _selectedIndex = index; }); },
                        children: new List<Widget> {
                            _buildOngoingEventList(context),
                            _buildCompletedEventList(context)
                        }
                    )
                )
            );
        }

        private IPromise onHeaderRefresh() {
            if (_selectedIndex == 0)
                pageNumber = 1;
            else
                completedPageNumber = 1;

            var tab = _selectedIndex == 0 ? "ongoing" : "completed";
            return EventApi.FetchEvents(_selectedIndex == 0 ? pageNumber : completedPageNumber, tab)
                .Then(eventsResponse => {
                    StoreProvider.store.Dispatch(new FetchEventsSuccessAction
                        {events = eventsResponse.events.items, tab = tab, pageNumber = 1, total = eventsResponse.events.total});
                })
                .Catch(error => { Debug.Log(error); });
        }

        private IPromise onFooterRefresh() {
            if (_selectedIndex == 0)
                pageNumber++;
            else
                completedPageNumber++;
            var tab = _selectedIndex == 0 ? "ongoing" : "completed";
            return EventApi.FetchEvents(_selectedIndex == 0 ? pageNumber : completedPageNumber, tab)
                .Then(eventsResponse => {
                    StoreProvider.store.Dispatch(new FetchEventsSuccessAction {events = eventsResponse.events.items, tab = tab, total = eventsResponse.events.total});
                })
                .Catch(error => { Debug.Log(error); });
        }

        public override void dispose() {
            base.dispose();
            _pageController.dispose();
        }
    }
}