using System.Collections.Generic ;
using System.Diagnostics ;
using System.Linq ;
using Fish.Scripts.Utilities ;
using Godot ;

namespace Fish.Scripts.Nodes
{
  public class Boid : KinematicBody, ICell
  {
    [Export]
    private float _maxSpeed = 18 ;

    [Export]
    private float _minSpeed = 12 ;

    [Export]
    private float _targetForce = 0.3f ;

    [Export]
    private float _cohesion = 0.2f ;

    [Export]
    private float _alignment = 0.3f ;

    [Export]
    private float _separation = 1.7f ;

    [Export]
    public float ViewDistance = 5f ;

    [Export]
    private float _avoidDistance = 2f ;

    [Export]
    private int _maxFlockSize = 10 ;

    [Export]
    private float _screenAvoidForce = 1f ;

    private const string GraphicsPath = "Graphics" ;
    private const string AnimationPlayerPath = "Graphics/AnimationPlayer" ;
    private const string AnimationSwimmingFastName = "Swimming_Fast" ;
    private const string VisibilityNotifierPath = "VisibilityNotifier" ;
    private const string ScreenEnteredEventName = "screen_entered" ;
    private const string ScreenExitedEventName = "screen_exited" ;
    private Spatial _graphics ;
    private VisibilityNotifier _visibilityNotifier ;
    private AnimationPlayer _animationPlayer ;
    private Vector2 _screenSize ;
    private readonly bool _stayOnScreen = true ;
    private readonly List<Vector3> _targets = new List<Vector3>() ;
    private float _raiseDegreesX ;
    private float _raiseDegreesZ ;
    private Vector3 _velocity ;
    public ICollection<Boid> Flock { get ; set ; }
    public bool IsDisabled { get ; private set ; }

    public override void _Ready()
    {
      // _screenSize = GetViewport().Size ;
      _screenSize = GraphicsExtensions.GameWorldScreenSize ;
      _graphics = GetNode<Spatial>( GraphicsPath ) ;
      _animationPlayer = GetNode<AnimationPlayer>( AnimationPlayerPath ) ;
      _animationPlayer.Play( AnimationSwimmingFastName ) ;
      _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
      GD.Randomize() ;
      _animationPlayer.PlaybackSpeed = (float) GD.RandRange( 2.5d, 3d ) ;
      _visibilityNotifier = GetNode<VisibilityNotifier>( VisibilityNotifierPath ) ;
      if ( _visibilityNotifier != null ) {
        _visibilityNotifier.Connect( ScreenEnteredEventName, this, nameof( ShowBoid ) ) ;
        _visibilityNotifier.Connect( ScreenExitedEventName, this, nameof( HideBoid ) ) ;
        Visible = false ;
      }

      GD.Randomize() ;
      _raiseDegreesX = (float) GD.RandRange( 10, 60 ) ;
      GD.Randomize() ;
      _raiseDegreesZ = (float) GD.RandRange( 5, 30 ) ;
      _velocity = new Vector3( (float) GD.RandRange( -1d, 1d ), (float) GD.RandRange( -1d, 1d ), 0 ).Floor() * _maxSpeed ;
      base._Ready() ;
    }

    private void ShowBoid()
    {
      Visible = true ;
    }

    private void HideBoid()
    {
      Visible = false ;
    }

    public override void _PhysicsProcess( float delta )
    {
      if ( IsDisabled ) return ;
      _velocity = CalculateLinearVelocity( _velocity ) ;
      MoveAndSlide( _velocity, Vector3.Up ) ;
      base._PhysicsProcess( delta ) ;
    }

    private Vector3 CalculateLinearVelocity( Vector3 velocity )
    {
      var screenAvoidVector = Vector3.Zero ;
      if ( _stayOnScreen ) screenAvoidVector = AvoidScreenEdge() * _screenAvoidForce ;
      else WrapScreen() ;
      var (cohesionVector, alignVector, separationVector) = GetFlockStatus() ;
      cohesionVector *= _cohesion ;
      alignVector *= _alignment ;
      separationVector *= _separation ;
      var additionalVelocity = cohesionVector + alignVector + separationVector + screenAvoidVector ;
      if ( _targets.Count > 0 ) {
        var targetVector = _targets.Aggregate( Vector3.Zero, ( current, target ) => current + GlobalTranslation.DirectionTo( target ) ) / _targets.Count ;
        additionalVelocity += targetVector * _targetForce ;
      }

      // Keep fish swims in 2D by converging Translation.Z to 0
      velocity.z = -Translation.z ;
      velocity = velocity.LinearInterpolate( velocity + additionalVelocity, 0.2f ).LimitLength( _maxSpeed ) ;
      if ( velocity.Length() < _minSpeed ) ( velocity * _minSpeed ).LimitLength( _maxSpeed ) ;
      velocity.Flip( _graphics, _raiseDegreesX, _raiseDegreesZ ) ;
      return velocity ;
    }

    private Vector3 AvoidScreenEdge()
    {
      var edgeAvoidVector = Vector3.Zero ;
      if ( Translation.x - _avoidDistance < 0 ) edgeAvoidVector.x = 1 ;
      else if ( Translation.x + _avoidDistance > _screenSize.x ) edgeAvoidVector.x = -1 ;
      if ( Translation.y - _avoidDistance < 0 ) edgeAvoidVector.y = 1 ;
      else if ( Translation.y + _avoidDistance > _screenSize.y ) edgeAvoidVector.y = -1 ;
      return edgeAvoidVector ;
    }

    private void WrapScreen()
    {
      Translation = new Vector3( Mathf.Wrap( Translation.x, 0, _screenSize.x ), Mathf.Wrap( Translation.y, 0, _screenSize.y ), 0 ) ;
    }

    private (Vector3 center, Vector3 align, Vector3 avoid) GetFlockStatus()
    {
      var (centerVector, flockCenter, alignVector, avoidVector, otherCount) = ( Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0 ) ;
      if ( Flock is null ) return ( centerVector, alignVector, avoidVector ) ;

      foreach ( var node in Flock ) {
        if ( otherCount == _maxFlockSize ) break ;
        if ( node == this ) continue ;
        if ( node == null || node.IsInsideTree() == false ) {
          continue ;
        }

        var otherPosition = node.GlobalTranslation ;
        var otherVelocity = node._velocity ;
        if ( ! ( GlobalTranslation.DistanceTo( otherPosition ) is var distance ) || ! ( distance < ViewDistance ) ) continue ;
        otherCount += 1 ;
        alignVector += otherVelocity ;
        flockCenter += otherPosition ;
        if ( distance < _avoidDistance ) avoidVector -= otherPosition - GlobalTranslation ;
      }

      if ( otherCount > 0 ) {
        alignVector /= otherCount ;
        flockCenter /= otherCount ;
        centerVector = GlobalTranslation.DirectionTo( flockCenter ) ;
      }

      return ( centerVector, alignVector, avoidVector ) ;
    }

    public void AddTarget( Vector3 targetPosition )
    {
      _targets.Add( targetPosition ) ;
    }

    public void ClearTarget()
    {
      _targets.Clear() ;
    }

    public void ReturnToPool()
    {
      HideBoid() ;
      IsDisabled = true ;
      SetPhysicsProcess( false ) ;
      SetProcess( false ) ;
      GetParent()?.RemoveChild( this ) ;
      // Translation = ( GetViewport().Size * -1 ).ToVector3( 0 ) ;
      // ResetPhysicsInterpolation() ;
    }

    public Vector2 ScaledPoint { get ; set ; }
  }
}