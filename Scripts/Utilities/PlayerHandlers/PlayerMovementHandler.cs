using Fish.Scripts.Nodes ;
using Godot ;

namespace Fish.Scripts.Utilities.PlayerHandlers
{
  class PlayerMovementHandler
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
        _moveDirection = new Vector3( _moveDirection.x, Mathf.Clamp( _moveDirection.y, -maxY, maxY ),
          _moveDirection.z ) ;
      }

      _owner.MoveAndSlide( _moveDirection * moveSpeed, Vector3.Up ) ;
      // Limit player in playable area
      _owner.Translation =
        _owner.Translation.Clamp( Vector3.Zero, GraphicsExtensions.GameWorldScreenSize.ToVector3( 0 ) ) ;
      return _moveDirection ;
    }

    public Vector3 MoveAndFlip( Vector3 toDirection, bool doDash )
    {
      var movementVector = Move( toDirection, doDash ) ;
      movementVector.Flip( _graphics, RaisedDegreesX, RaisedDegreesZ ) ;
      return movementVector ;
    }
  }
}