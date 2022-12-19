using Godot ;
using System.Collections.Generic ;
using Fish.Utilities ;

public class Player : KinematicBody
{
  private const string AnimationPlayerPath = "Graphics/AnimationPlayer" ;
  private const string GraphicsPath = "Graphics" ;
  private const string DashPath = "Dash" ;
  private const float MoveSpeed = 30f ;
  private const float DashDuration = .1f ;
  private const float DashMultiplier = 3f ;
  private const float RaisedDegrees = 30f ;
  private AnimationPlayer _animationPlayer ;
  private Spatial _graphics ;
  private Dash _dash ;

  private readonly List<(string, Vector3)> _actionInfo = new List<(string, Vector3)>()
  {
    ( "ui_right", new Vector3( 1, 0, 0 ) ),
    ( "ui_left", new Vector3( -1, 0, 0 ) ),
    ( "ui_up", new Vector3( 0, 1, 0 ) ),
    ( "ui_down", new Vector3( 0, -1, 0 ) ),
  } ;

  public override void _Ready()
  {
    _animationPlayer = GetNode<AnimationPlayer>( AnimationPlayerPath ) ;
    _graphics = GetNode<Spatial>( GraphicsPath ) ;
    _dash = GetNode<Dash>( DashPath ) ;
  }

  public override void _PhysicsProcess( float delta )
  {
    Move().Flip( _graphics, RaisedDegrees ) ;
    base._PhysicsProcess( delta ) ;
  }

  private Vector3 Move()
  {
    var moveDirection = Vector3.Zero ;
    foreach ( var (action, direction) in _actionInfo ) {
      if ( Input.IsActionPressed( action ) ) moveDirection += direction ;
    }

    if ( Input.IsActionPressed( "ui_dash" ) && _dash.CanDash ) _dash.StartDash( DashDuration ) ;
    var moveSpeed = MoveSpeed ;
    if ( _dash.IsDashing ) {
      moveSpeed *= DashMultiplier ;
      // Limit vertical movement because I think fish can not move quickly in vertical
      var maxY = Mathf.Abs( Mathf.Sin( Mathf.Deg2Rad( RaisedDegrees ) ) ) ;
      moveDirection = new Vector3( moveDirection.x, Mathf.Clamp( moveDirection.y, -maxY, maxY ), moveDirection.z ) ;
    }

    MoveAndSlide( moveDirection * moveSpeed, Vector3.Up ) ;
    return moveDirection ;
  }

  private void Flip( Vector3 moveDirection )
  {
    const float flipDegrees = 90f ;
    const float weight = 0.15f ;
    var rotationDegreesY = moveDirection.x > 0 ? flipDegrees : moveDirection.x < 0 ? -flipDegrees : _graphics.RotationDegrees.y ;
    // This can be simplified to just the X axis, but I added the Z axis to illustrate 2.5D movement (better visual)
    var rotationDegreesXnZ = moveDirection.y > 0 ? -RaisedDegrees : moveDirection.y < 0 ? RaisedDegrees : 0 ;
    var lerpX = Mathf.Lerp( _graphics.RotationDegrees.x, rotationDegreesXnZ, weight ) ;
    var lerpY = Mathf.Lerp( _graphics.RotationDegrees.y, rotationDegreesY, weight ) ;
    var lerpZ = Mathf.Lerp( _graphics.RotationDegrees.z, rotationDegreesXnZ, weight ) ;
    _graphics.RotationDegrees = new Vector3( lerpX, lerpY, lerpZ ) ;
  }
}