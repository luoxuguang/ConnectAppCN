using System;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.components.refresh {
    public delegate IPromise RefresherCallback();

    public delegate RectTween CreateTween(RefreshWidget widget);

    public enum RefreshState {
        drag,
        ready,
        loading
    }

    public class RefreshWidgetController : ValueNotifier<float> {
        public RefreshWidgetController(
            float value = 0,
            ValueNotifier<RefreshState> _state = null
        ) : base(value) {
            this.value = value;
            this._state = _state ?? new ValueNotifier<RefreshState>(RefreshState.drag);
        }

        public readonly ValueNotifier<RefreshState> _state;

        public RefreshState state {
            get => _state.value;
            set => _state.value = value;
        }

        public bool loading => _state.value == RefreshState.loading;

        public void addStateListener(VoidCallback updateState) {
            _state.addListener(updateState);
        }

        public void removeStateListener(VoidCallback updateState) {
            _state.removeListener(updateState);
        }

        public override void dispose() {
            _state.dispose();
            base.dispose();
        }
    }


    public class RefreshWidget : StatefulWidget {
        public readonly float height;

        public readonly float maxOffset;

        public readonly RefreshChildBuilder childBuilder;

        public readonly RefreshWidgetController controller;

        public readonly CreateTween createTween;

        public readonly Alignment alignment;


        public RefreshWidget(
            float height,
            float maxOffset,
            RefreshChildBuilder childBuilder,
            RefreshWidgetController controller,
            CreateTween createTween,
            Alignment alignment,
            Key key = null
        ) : base(key) {
            this.height = height;
            this.maxOffset = maxOffset;
            this.childBuilder = childBuilder;
            this.controller = controller;
            this.createTween = createTween;
            this.alignment = alignment;
        }

        public override State createState() {
            return new _RefreshHeaderState();
        }
    }


    internal class _RefreshHeaderState : State<RefreshWidget>, TickerProvider {
        private AnimationController _positionController;

        //
        private Animation<Rect> _positionFactor;

        public override void initState() {
            base.initState();
            _positionController = new AnimationController(vsync: this);
            _positionFactor = widget.createTween(widget).animate(_positionController);
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            if (oldWidget is RefreshWidget) {
                RefreshWidget refreshWidget = (RefreshWidget) oldWidget;
                if (refreshWidget.controller != widget.controller) {
                    refreshWidget.controller.removeListener(_updateValue);
                    refreshWidget.controller.removeStateListener(_updateState);
                    widget.controller.addListener(_updateValue);
                    widget.controller.addStateListener(_updateState);
                }
            }

            base.didUpdateWidget(oldWidget);
        }

        public override void didChangeDependencies() {
            widget.controller.addListener(_updateValue);
            widget.controller.addStateListener(_updateState);
            base.didChangeDependencies();
        }

        public override void dispose() {
            widget.controller.removeListener(_updateValue);
            widget.controller.removeStateListener(_updateState);
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            RefreshWidget widget = this.widget;
            return new RelativePositionedTransition(
                size: new Size(0, 0),
                rect: _positionFactor,
                child: new AnimatedBuilder(
                    animation: _positionController,
                    builder: (BuildContext _context, Widget child) => {
                        return new Align(
                            child: new SizedBox(
                                height: widget.height,
                                child: widget.childBuilder(_context, widget.controller)
                            ),
                            alignment: widget.alignment
                        );
                    }
                )
            );
        }

        public Ticker createTicker(TickerCallback onTick) {
            Ticker _ticker = new Ticker(onTick, debugLabel: $"created by {this}");
            return _ticker;
        }


        private void _updateValue() {
            float value = widget.controller.value / (widget.maxOffset + widget.height);
            //let's move head
            _positionController.setValue(value);
        }

        private void _updateState() {
            switch (widget.controller.state) {
                case RefreshState.drag:
                    break;
                case RefreshState.loading: {
                    float value = widget.height / (widget.maxOffset + widget.height);
                    _positionController
                        .animateTo(value,
                            duration: new TimeSpan(0, 0, 0, 0, 300), curve: Curves.ease)
                        .Done(() => { });
                }
                    break;
                case RefreshState.ready:
                    break;
            }
        }
    }
}