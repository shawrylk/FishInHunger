using System ;
using System.Collections.Generic ;
using System.Threading.Tasks ;
using Godot ;

namespace Fish.Scripts.Utilities.PlayerHandlers
{
  abstract class PlayerInputHandler
  {
    private const string DashActionName = "ui_dash" ;

    protected static async void CreatePseudoDashActionWithTask()
    {
      Input.ActionPress( DashActionName ) ;
      await Task.Delay( TimeSpan.FromSeconds( 0.1f ) ) ;
      Input.ActionRelease( DashActionName ) ;
    }

    public virtual void UnhandledInput( InputEvent @event )
    {
    }

    public virtual Vector3 GetMovementDirection() => Vector3.Zero ;
    public static bool IsActionPressed() => Input.IsActionPressed( DashActionName ) ;
  }

  class MouseHandler : PlayerInputHandler
  {
    private Vector2? _lastMouseDirection ;

    public MouseHandler()
    {
      Input.MouseMode = Input.MouseModeEnum.Captured ;
    }

    public override void UnhandledInput( InputEvent @event )
    {
      switch ( @event ) {
        case InputEventMouseMotion eventMouseMotion :
          _lastMouseDirection = eventMouseMotion.Relative ;
          break ;
        case InputEventMouseButton { Pressed: true } :
          CreatePseudoDashActionWithTask() ;
          break ;
      }
    }

    private Vector2 GetDirection()
    {
      var result = _lastMouseDirection ?? Vector2.Zero ;
      _lastMouseDirection = null ;
      return result ;
    }

    public override Vector3 GetMovementDirection()
    {
      var direction = GetDirection().Normalized() ;
      return new Vector3( direction.x, -direction.y, 0 ) ;
    }
  }

  class TouchHandler : PlayerInputHandler
  {
    private Vector2 _lastTouchPosition ;
    private DateTime _lastTouchTime ;
    private Vector2? _lastMouseDirection ;

    public override void UnhandledInput( InputEvent @event )
    {
      switch ( @event ) {
        case InputEventScreenTouch eventScreenTouch :
          if ( eventScreenTouch.Pressed && eventScreenTouch.Index == 0 ) {
            _lastTouchPosition = eventScreenTouch.Position ;
          }

          if ( eventScreenTouch.Pressed && eventScreenTouch.Index == 1 ) {
            var now = DateTime.Now ;
            if ( now - _lastTouchTime < TimeSpan.FromSeconds( 0.5f ) ) CreatePseudoDashActionWithTask() ;
            _lastTouchTime = now ;
          }

          break ;
        case InputEventScreenDrag eventScreenDrag :
          _lastMouseDirection = eventScreenDrag.Position - _lastTouchPosition ;
          break ;
      }
    }

    private Vector2 GetDirection()
    {
      var result = _lastMouseDirection ?? Vector2.Zero ;
      _lastMouseDirection = null ;
      return result ;
    }

    public override Vector3 GetMovementDirection()
    {
      var direction = GetDirection().Normalized() ;
      return new Vector3( direction.x, -direction.y, 0 ) ;
    }
  }

  class KeyboardHandler : PlayerInputHandler
  {
    private static readonly List<(string, Vector3)> ActionInfo = new List<(string, Vector3)>()
    {
      ( "ui_right", new Vector3( 1, 0, 0 ) ),
      ( "ui_left", new Vector3( -1, 0, 0 ) ),
      ( "ui_up", new Vector3( 0, 1, 0 ) ),
      ( "ui_down", new Vector3( 0, -1, 0 ) ),
    } ;

    public override Vector3 GetMovementDirection()
    {
      var moveDirection = Vector3.Zero ;
      foreach ( var (action, direction) in ActionInfo ) {
        if ( Input.IsActionPressed( action ) ) moveDirection += direction ;
      }

      return moveDirection ;
    }
  }
}