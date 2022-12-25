using System ;
using Godot ;
using System.Collections.Generic ;
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
  private const float MoveSpeed = 60f ;
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

  public override void _UnhandledInput( InputEvent @event )
  {
    if ( @event is InputEventMouseMotion eventMouseMotion ) {
      _setMouseDirection( eventMouseMotion.Relative ) ;
    }

    base._UnhandledInput( @event ) ;
  }

  private Vector3 Move()
  {
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

    var moveDirection = KeyboardHandler() + MouseHandler( this ) ;
    // To simulate that fish swims faster when moving
    _animationPlayer.PlaybackSpeed = moveDirection == Vector3.Zero ? 1f : 4f ;

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
      moveDirection = new Vector3( moveDirection.x, Mathf.Clamp( moveDirection.y, -maxY, maxY ), moveDirection.z ) ;
    }

    MoveAndSlide( moveDirection * moveSpeed, Vector3.Up ) ;
    return moveDirection ;
  }

  private void OnAnimationFinished( string animationName )
  {
    _animationPlayer.Disconnect( AnimationFinishedEventName, this, animationName ) ;
    _animationPlayer.Play( AnimationSwimmingFastName ) ;
    _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
  }
}