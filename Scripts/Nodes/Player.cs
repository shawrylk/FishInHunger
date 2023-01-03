using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;
using Fish.Scripts.Utilities ;
using Godot ;

namespace Fish.Scripts.Nodes
{
  public class Player : KinematicBody
  {
    private readonly IList<PlayerInputHandler> _playerActionHandlers = new List<PlayerInputHandler>() ;
    private PlayerMovementHandler _movementHandler ;
    private AnimationHandler _animationHandler ;
    private CollidingHandler _collidingHandler ;

    public override void _Ready()
    {
      _movementHandler = new PlayerMovementHandler( this ) ;
      _animationHandler = new AnimationHandler( this ) ;
      _collidingHandler = new CollidingHandler( this ) ;
      _playerActionHandlers.Add( new KeyboardHandler() ) ;
      _playerActionHandlers.Add( new TouchHandler() ) ;
      // Because touch action on Mobile conflicts with mouse, and I don't know what will happen to Desktop has touch support,
      // So it will be check later if bug occurs
      if ( OS.GetName() != "Android" && OS.GetName() != "iOS" ) {
        _playerActionHandlers.Add( new MouseHandler() ) ;
      }
    }

    public override void _PhysicsProcess( float delta )
    {
      var toDirection = _playerActionHandlers.Aggregate( Vector3.Zero, ( current, handler ) => current + handler.GetMovementDirection() ) ;
      var doDash = _playerActionHandlers.Any( handler => PlayerInputHandler.IsActionPressed() ) ;
      var moveDirection = _movementHandler.MoveAndFlip( toDirection, doDash ) ;
      _animationHandler.UpdateAnimation( moveDirection, _movementHandler.IsDashing ) ;
      _collidingHandler.HandleCollider() ;
      base._PhysicsProcess( delta ) ;
    }

    public override void _UnhandledInput( InputEvent @event )
    {
      foreach ( var playerActionHandler in _playerActionHandlers ) {
        playerActionHandler.UnhandledInput( @event ) ;
      }

      base._UnhandledInput( @event ) ;
    }

// #if DEBUG
//     public override void _Process( float delta )
//     {
//       GD.Print( Engine.GetFramesPerSecond() ) ;
//       base._Process( delta ) ;
//     }
// #endif

    private abstract class PlayerInputHandler
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

    private class MouseHandler : PlayerInputHandler
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

    private class TouchHandler : PlayerInputHandler
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

    private class KeyboardHandler : PlayerInputHandler
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

    private class PlayerMovementHandler
    {
      private const string GraphicsPath = "Graphics" ;
      private const string DashPath = "Dash" ;
      private const float MoveSpeed = 30f ;
      private const float DashDuration = .05f ;
      private const float DashMultiplier = 3f ;
      private const float RaisedDegreesX = 35f ;
      private const float RaisedDegreesZ = 25f ;
      private const float InterpolateWeight = 0.2f ;
      private readonly KinematicBody _owner ;
      private readonly Dash _dash ;
      private readonly Spatial _graphics ;
      private Vector3 _moveDirection ;
      public bool IsDashing { get ; private set ; }

      public PlayerMovementHandler( KinematicBody owner )
      {
        _owner = owner ;
        _dash = owner.GetNode<Dash>( DashPath ) ;
        _graphics = owner.GetNode<Spatial>( GraphicsPath ) ;
      }

      private Vector3 Move( Vector3 toDirection, bool doDash )
      {
        IsDashing = false ;
        _moveDirection = _moveDirection.LinearInterpolate( toDirection, InterpolateWeight ) ;

        if ( doDash && _dash.CanDash ) {
          IsDashing = true ;
          _dash.StartDash( DashDuration ) ;
        }

        var moveSpeed = MoveSpeed ;
        if ( _dash.IsDashing ) {
          moveSpeed *= DashMultiplier ;
          // Limit vertical movement because I think fish can not move quickly in vertical
          var maxY = Mathf.Abs( Mathf.Sin( Mathf.Deg2Rad( RaisedDegreesX ) ) ) ;
          _moveDirection = new Vector3( _moveDirection.x, Mathf.Clamp( _moveDirection.y, -maxY, maxY ), _moveDirection.z ) ;
        }

        _owner.MoveAndSlide( _moveDirection * moveSpeed, Vector3.Up ) ;
        // Limit player in playable area
        _owner.Translation = _owner.Translation.Clamp( Vector3.Zero, GraphicsExtensions.GameWorldScreenSize.ToVector3( 0 ) ) ;
        return _moveDirection ;
      }

      public Vector3 MoveAndFlip( Vector3 toDirection, bool doDash )
      {
        var movementVector = Move( toDirection, doDash ) ;
        movementVector.Flip( _graphics, RaisedDegreesX, RaisedDegreesZ ) ;
        return movementVector ;
      }
    }

    private class AnimationHandler
    {
      private const string AnimationPlayerPath = "Graphics/AnimationPlayer" ;
      private const string AnimationSwimmingFastName = "Swimming_Fast" ;
      private const string AnimationSwimmingImpulseName = "Attack" ;

      private readonly AnimationPlayer _animationPlayer ;

      public AnimationHandler( Node owner )
      {
        _animationPlayer = owner.GetNode<AnimationPlayer>( AnimationPlayerPath ) ;
        _animationPlayer.Play( AnimationSwimmingFastName ) ;
        _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
      }

      public void UpdateAnimation( Vector3 moveDirection, bool isDashing )
      {
        // To simulate that fish swims faster when moving
        _animationPlayer.PlaybackSpeed = moveDirection == Vector3.Zero ? 1f : 1.7f ;
        if ( isDashing ) _animationPlayer.Play( AnimationSwimmingImpulseName ) ;
        if ( _animationPlayer.IsPlaying() ) return ;
        _animationPlayer.Play( AnimationSwimmingFastName ) ;
        _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
      }
    }

    private class CollidingHandler
    {
      private readonly KinematicBody _owner ;

      public CollidingHandler( KinematicBody owner )
      {
        _owner = owner ;
      }

      public void HandleCollider()
      {
        for ( int index = 0, count = _owner.GetSlideCount() ; index < count ; index++ ) {
          var collision = _owner.GetSlideCollision( index ) ;
          if ( collision.Collider is Boid boid && boid.IsInGroup( RandomSpawn.BoidsGroupNodePath ) ) {
            boid.ReturnToPool() ;
          }
        }
      }
    }
  }
}