using System ;
using Godot ;
using System.Collections.Generic ;
using System.Threading.Tasks ;
using Fish.Utilities ;

public class Player : KinematicBody
{
  private const string AnimationPlayerPath = "Graphics/AnimationPlayer" ;
  private const string AnimationSwimmingFastName = "Swimming_Fast" ;

  private const string AnimationSwimmingImpulseName = "Swimming_Impulse" ;

  // private const string AnimationAttackName = "Attack" ;
  private const string AnimationFinishedEventName = "animation_finished" ;

  private const string GraphicsPath = "Graphics" ;

  // private const string CameraPath = "Camera" ;
  private const string DashPath = "Dash" ;
  private const float MoveSpeed = 30f ;
  private const float DashDuration = .1f ;
  private const float DashMultiplier = 3f ;
  private const float RaisedDegreesX = 35f ;
  private const float RaisedDegreesZ = 25f ;
  private AnimationPlayer _animationPlayer ;
  private Spatial _graphics ;

  private Dash _dash ;

  // private Camera _camera ;
  private Func<Vector2> _getMouseDirection ;
  private Action<Vector2> _setMouseDirection ;

  private static readonly List<(string, Vector3)> ActionInfo = new List<(string, Vector3)>()
  {
    ( "ui_right", new Vector3( 1, 0, 0 ) ),
    ( "ui_left", new Vector3( -1, 0, 0 ) ),
    ( "ui_up", new Vector3( 0, 1, 0 ) ),
    ( "ui_down", new Vector3( 0, -1, 0 ) ),
  } ;

  public override void _Ready()
  {
    _animationPlayer = GetNode<AnimationPlayer>( AnimationPlayerPath ) ;
    _animationPlayer.Play( AnimationSwimmingFastName ) ;
    _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
    _graphics = GetNode<Spatial>( GraphicsPath ) ;
    _dash = GetNode<Dash>( DashPath ) ;
    // _camera = GetNode<Camera>( CameraPath ) ;
    Input.MouseMode = Input.MouseModeEnum.Captured ;
    ( _getMouseDirection, _setMouseDirection ) = GetMouseHelperFunction() ;
  }

  public override void _PhysicsProcess( float delta )
  {
    Move().Flip( _graphics, RaisedDegreesX, RaisedDegreesZ ) ;
    base._PhysicsProcess( delta ) ;
  }

  private static (Func<Vector2> get, Action<Vector2> set) GetMouseHelperFunction()
  {
    Vector2? lastMouseDirection = null ;

    Vector2 GetDirection()
    {
      var result = lastMouseDirection ?? Vector2.Zero ;
      lastMouseDirection = null ;
      return result ;
    }

    void SetDirection( Vector2 newDirection )
    {
      lastMouseDirection = newDirection ;
    }

    return ( GetDirection, SetDirection ) ;
  }

// #if DEBUG
//   public override void _Process( float delta )
//   {
//     GD.Print( Engine.GetFramesPerSecond() ) ;
//     base._Process( delta ) ;
//   }
// #endif

  private Vector2 _lastTouchPosition ;
  private DateTime _lastTouchTime ;

  private void CreatePseudoDashActionWithTask()
  {
    _ = Task.Run( async () =>
    {
      GD.Print( "ui_dash press" ) ;
      Input.ActionPress( "ui_dash" ) ;
      await this.Wait( 0.1f ) ;
      GD.Print( "ui_dash release" ) ;
      Input.ActionRelease( "ui_dash" ) ;
    } ) ;
  }

  public override async void _UnhandledInput( InputEvent @event )
  {
    switch ( @event ) {
      case InputEventMouseMotion eventMouseMotion :
        _setMouseDirection( eventMouseMotion.Relative ) ;
        break ;
      case InputEventMouseButton { Doubleclick: true } :
        CreatePseudoDashActionWithTask() ;
        break ;
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
        _setMouseDirection( eventScreenDrag.Position - _lastTouchPosition ) ;
        break ;
    }

    base._UnhandledInput( @event ) ;
  }

  private Vector3 _moveDirection ;

  private Vector3 Move()
  {
    _moveDirection = _moveDirection.LinearInterpolate( KeyboardHandler() + MouseHandler( this ), 0.2f ) ;
    // To simulate that fish swims faster when moving
    _animationPlayer.PlaybackSpeed = _moveDirection == Vector3.Zero ? 1f : 2f ;

    if ( Input.IsActionPressed( "ui_dash" ) && _dash.CanDash ) {
      _dash.StartDash( DashDuration ) ;
      _animationPlayer.Play( AnimationSwimmingImpulseName ) ;
      if ( ! _animationPlayer.IsConnected( AnimationFinishedEventName, this, nameof( OnAnimationFinished ) ) )
        _animationPlayer.Connect( AnimationFinishedEventName, this, nameof( OnAnimationFinished ) ) ;
    }

    var moveSpeed = MoveSpeed ;
    if ( _dash.IsDashing ) {
      moveSpeed *= DashMultiplier ;
      // Limit vertical movement because I think fish can not move quickly in vertical
      var maxY = Mathf.Abs( Mathf.Sin( Mathf.Deg2Rad( RaisedDegreesX ) ) ) ;
      _moveDirection = new Vector3( _moveDirection.x, Mathf.Clamp( _moveDirection.y, -maxY, maxY ), _moveDirection.z ) ;
    }

    MoveAndSlide( _moveDirection * moveSpeed, Vector3.Up ) ;
    return _moveDirection ;

    static Vector3 KeyboardHandler()
    {
      var moveDirection = Vector3.Zero ;
      foreach ( var (action, direction) in ActionInfo ) {
        if ( Input.IsActionPressed( action ) ) moveDirection += direction ;
      }

      return moveDirection ;
    }

    static Vector3 MouseHandler( Player player )
    {
      var direction = player._getMouseDirection().Normalized() ;
      return new Vector3( direction.x, -direction.y, 0 ) ;
    }
  }

  private void OnAnimationFinished( string animationName )
  {
    _animationPlayer.Disconnect( AnimationFinishedEventName, this, nameof( OnAnimationFinished ) ) ;
    _animationPlayer.Play( AnimationSwimmingFastName ) ;
    _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
  }
}