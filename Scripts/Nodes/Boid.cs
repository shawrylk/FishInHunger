using System.Collections.Generic ;
using System.Linq ;
using Fish.Scripts.Utilities ;
using Godot ;

namespace Fish.Scripts.Nodes
{
<<<<<<< Updated upstream
  public class Boid : Spatial
=======
  public class Boid : Spatial, ICell
>>>>>>> Stashed changes
  {
    // [Export]
    private static float _maxSpeed = 18 ;

    // [Export]
    private static float _minSpeed = 12 ;

    // [Export]
    private static float _targetForce = 0.3f ;

    // [Export]
    private static float _cohesion = 0.2f ;

    // [Export]
    private static float _alignment = 0.3f ;

    // [Export]
    private static float _separation = 1.7f ;

    // [Export]
    public static float ViewDistance = 5f ;

    // [Export]
    private static float _avoidDistance = 2f ;

    // [Export]
    private static int _maxFlockSize = 10 ;

    // [Export]
    private static float _screenAvoidForce = 1f ;

    private const string GraphicsPath = "Graphics" ;
    private const string AnimationPlayerPath = "AnimationPlayer" ;

    private const string AnimationSwimmingFastName = "Swimming_Fast" ;

    // private const string VisibilityNotifierPath = "VisibilityNotifier" ;
    private const string ScreenEnteredEventName = "screen_entered" ;
    private const string ScreenExitedEventName = "screen_exited" ;
    private VisibilityNotifier _visibilityNotifier ;
    private AnimationPlayer _animationPlayer ;
    private Vector2 _screenSize ;
<<<<<<< Updated upstream
    private Vector3 _velocity ;
    private readonly bool _stayOnScreen = true ;
    private readonly List<Vector3> _targets = new List<Vector3>() ;
=======
    private static readonly bool _stayOnScreen = true ;
    private static readonly List<Vector3> _targets = new List<Vector3>() ;
>>>>>>> Stashed changes
    private float _raiseDegreesX ;
    private float _raiseDegreesZ ;
    public List<List<Boid>> Flock { get ; set ; }

    public override void _Ready()
    {
      Rotate( Vector3.Up, 90 ) ;
      // _screenSize = GetViewport().Size ;
      _screenSize = GraphicsExtensions.GameWorldScreenSize ;
      _animationPlayer = GetNode<AnimationPlayer>( AnimationPlayerPath ) ;
      _animationPlayer.Play( AnimationSwimmingFastName ) ;
      _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
      GD.Randomize() ;
      _animationPlayer.PlaybackSpeed = (float) GD.RandRange( 2.5d, 3d ) ;
      // _visibilityNotifier = GetNode<VisibilityNotifier>( VisibilityNotifierPath ) ;
      if ( _visibilityNotifier != null ) {
        _visibilityNotifier.Connect( ScreenEnteredEventName, this, nameof( ShowBoid ) ) ;
        _visibilityNotifier.Connect( ScreenExitedEventName, this, nameof( HideBoid ) ) ;
        Visible = false ;
      }

      GD.Randomize() ;
      _raiseDegreesX = (float) GD.RandRange( 10, 60 ) ;
      GD.Randomize() ;
      _raiseDegreesZ = (float) GD.RandRange( 5, 30 ) ;
      _velocity = new Vector3( (float) GD.RandRange( -1d, 1d ), (float) GD.RandRange( -1d, 1d ), 0 ) * _maxSpeed ;
      base._Ready() ;
    }

    private void ShowBoid()
    {
      Visible = true ;
    }

    private void HideBoid()
    {
      Visible = true ;
    }

<<<<<<< Updated upstream
    public override void _PhysicsProcess( float delta )
    {
      Translation += _velocity * delta ;
=======
    // public override void _PhysicsProcess( float delta )
    // {
    //   if ( IsDisabled ) return ;
    //   _velocity = CalculateLinearVelocity( this ) ;
    //   MoveAndSlide( _velocity, Vector3.Up ) ;
    //   base._PhysicsProcess( delta ) ;
    // }

    public static Vector3 CalculateLinearVelocity( Boid instance )
    {
      var velocity = instance._velocity ;
>>>>>>> Stashed changes
      var screenAvoidVector = Vector3.Zero ;
      if ( _stayOnScreen ) screenAvoidVector = AvoidScreenEdge( instance ) * _screenAvoidForce ;
      else WrapScreen( instance ) ;
      var (cohesionVector, alignVector, separationVector) = GetFlockStatus( instance ) ;
      cohesionVector *= _cohesion ;
      alignVector *= _alignment ;
      separationVector *= _separation ;
      var additionalVelocity = cohesionVector + alignVector + separationVector + screenAvoidVector ;
      if ( _targets.Count > 0 ) {
        var targetVector = _targets.Aggregate( Vector3.Zero, ( current, target ) => current + instance.GlobalTranslation.DirectionTo( target ) ) / _targets.Count ;
        additionalVelocity += targetVector * _targetForce ;
      }

<<<<<<< Updated upstream
      _velocity = _velocity.LinearInterpolate( _velocity + additionalVelocity, 0.2f ).LimitLength( _maxSpeed ) ;
      if ( _velocity.Length() < _minSpeed ) ( _velocity * _minSpeed ).LimitLength( _maxSpeed ) ;
      _velocity.Flip( _graphics, _raiseDegreesX, _raiseDegreesZ ) ;
      base._PhysicsProcess( delta ) ;
=======
      // Keep fish swims in 2D by converging Translation.Z to 0
      velocity.z = -instance.Translation.z ;
      velocity = velocity.LinearInterpolate( velocity + additionalVelocity, 0.2f ).LimitLength( _maxSpeed ) ;
      if ( velocity.Length() < _minSpeed ) ( velocity * _minSpeed ).LimitLength( _maxSpeed ) ;
      velocity.Flip( instance, instance._raiseDegreesX, instance._raiseDegreesZ ) ;
      return velocity ;
>>>>>>> Stashed changes
    }

    private static Vector3 AvoidScreenEdge( Boid instance )
    {
      var edgeAvoidVector = Vector3.Zero ;
      if ( instance.Translation.x - _avoidDistance < 0 ) edgeAvoidVector.x = 1 ;
      else if ( instance.Translation.x + _avoidDistance > instance._screenSize.x ) edgeAvoidVector.x = -1 ;
      if ( instance.Translation.y - _avoidDistance < 0 ) edgeAvoidVector.y = 1 ;
      else if ( instance.Translation.y + _avoidDistance > instance._screenSize.y ) edgeAvoidVector.y = -1 ;
      return edgeAvoidVector ;
    }

    private static void WrapScreen( Boid instance )
    {
      instance.Translation = new Vector3( Mathf.Wrap( instance.Translation.x, 0, instance._screenSize.x ), Mathf.Wrap( instance.Translation.y, 0, instance._screenSize.y ), 0 ) ;
    }

    private static (Vector3 center, Vector3 align, Vector3 avoid) GetFlockStatus( Boid instance )
    {
      var (centerVector, flockCenter, alignVector, avoidVector, otherCount) = ( Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0 ) ;
<<<<<<< Updated upstream
      if ( Flock is null ) return ( centerVector, alignVector, avoidVector ) ;
      foreach ( var listNode in Flock ) {
        foreach ( var node in listNode ) {
          if ( otherCount == _maxFlockSize ) break ;
          if ( node == this ) continue ;
          var otherPosition = node.GlobalTranslation ;
          var otherVelocity = node._velocity ;
          if ( ! ( GlobalTranslation.DistanceTo( otherPosition ) is var distance ) || ! ( distance < ViewDistance ) ) continue ;
          otherCount += 1 ;
          alignVector += otherVelocity ;
          flockCenter += otherPosition ;
          if ( distance < _avoidDistance ) avoidVector -= otherPosition - GlobalTranslation ;
        }
=======
      if ( instance.Flock is null ) return ( centerVector, alignVector, avoidVector ) ;

      foreach ( var node in instance.Flock ) {
        if ( otherCount == _maxFlockSize ) break ;
        if ( node == instance ) continue ;
        if ( node == null || node.IsInsideTree() == false ) {
          continue ;
        }

        var otherPosition = node.GlobalTranslation ;
        var otherVelocity = node._velocity ;
        if ( ! ( instance.GlobalTranslation.DistanceTo( otherPosition ) is var distance ) || ! ( distance < ViewDistance ) ) continue ;
        otherCount += 1 ;
        alignVector += otherVelocity ;
        flockCenter += otherPosition ;
        if ( distance < _avoidDistance ) avoidVector -= otherPosition - instance.GlobalTranslation ;
>>>>>>> Stashed changes
      }

      if ( otherCount > 0 ) {
        alignVector /= otherCount ;
        flockCenter /= otherCount ;
        centerVector = instance.GlobalTranslation.DirectionTo( flockCenter ) ;
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
  }
}