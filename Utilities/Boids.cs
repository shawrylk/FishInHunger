using System.Collections.Generic ;
using System.Linq ;
using Godot ;

namespace Fish.Utilities
{
  public class Boids : Node2D
  {
    [Export]
    private float _maxSpeed = 100 ;

    [Export]
    private float _minSpeed = 80 ;

    [Export]
    private float _targetForce = 2f ;

    [Export]
    private float _cohesion = 2f ;

    [Export]
    private float _alignment = 3f ;

    [Export]
    private float _separation = 5f ;

    [Export]
    private float _viewDistance = 50f ;

    [Export]
    private float _avoidDistance = 15f ;

    [Export]
    private int _maxFlockSize = 15 ;

    [Export]
    private float _screenAvoidForce = 10f ;

    private Vector2 _screenSize ;
    private bool _stayOnScreen = true ;
    private int _flockSize = 0 ;
    private List<Vector2> _targets = new List<Vector2>() ;
    public List<List<Boids>> Flock { get ; set ; }
    public Vector2 Velocity { get ; private set ; }

    public override void _Ready()
    {
      _screenSize = GetViewportRect().Size ;
      GD.Randomize() ;
      Velocity = new Vector2( (float) GD.RandRange( -1d, 1d ), (float) GD.RandRange( -1d, 1d ) ).Floor() * _maxSpeed ;
      base._Ready() ;
    }

    public override void _Process( float delta )
    {
      Position += Velocity * delta ;
      var screenAvoidVector = Vector2.Zero ;
      if ( _stayOnScreen ) screenAvoidVector = AvoidScreenEdge() * _screenAvoidForce ;
      else WrapScreen() ;
      var (cohesionVector, alignVector, separationVector, flockSize) = GetFlockStatus() ;
      cohesionVector = cohesionVector * _cohesion ;
      alignVector = alignVector * _alignment ;
      separationVector = separationVector * _separation ;
      _flockSize = flockSize ;
      var additionalVelocity = cohesionVector + alignVector + separationVector + screenAvoidVector ;
      if ( _targets.Count > 0 ) {
        var targetVector = _targets.Aggregate( Vector2.Zero, ( current, target ) => current + GlobalPosition.DirectionTo( target ) ) / _targets.Count ;
        additionalVelocity += targetVector * _targetForce ;
      }

      Velocity = ( Velocity + additionalVelocity ).LimitLength( _maxSpeed ) ;
      if ( Velocity.Length() < _minSpeed ) ( Velocity * _minSpeed ).LimitLength( _maxSpeed ) ;
      base._Process( delta ) ;
    }

    private Vector2 AvoidScreenEdge()
    {
      var edgeAvoidVector = Vector2.Zero ;
      if ( Position.x - _avoidDistance < 0 ) edgeAvoidVector.x = 1 ;
      else if ( Position.x + _avoidDistance > _screenSize.x ) edgeAvoidVector.x = -1 ;
      if ( Position.y - _avoidDistance < 0 ) edgeAvoidVector.y = 1 ;
      else if ( Position.y + _avoidDistance > _screenSize.y ) edgeAvoidVector.y = -1 ;
      return edgeAvoidVector ;
    }

    private void WrapScreen()
    {
      Position = new Vector2( Mathf.Wrap( Position.x, 0, _screenSize.x ), Mathf.Wrap( Position.y, 0, _screenSize.y ) ) ;
    }

    private (Vector2 center, Vector2 align, Vector2 avoid, int otherCount) GetFlockStatus()
    {
      var (centerVector, flockCenter, alignVector, avoidVector, otherCount) = ( Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, 0 ) ;
      foreach ( var listNode in Flock ) {
        foreach ( var node in listNode ) {
          if ( otherCount == _maxFlockSize ) break ;
          if ( node == this ) continue ;
          var otherPosition = node.GlobalPosition ;
          var otherVelocity = node.Velocity ;
          if ( ! ( GlobalPosition.DistanceTo( otherPosition ) is var distance ) || ! ( distance < _viewDistance ) ) continue ;
          otherCount += 1 ;
          alignVector += otherVelocity ;
          flockCenter += otherPosition ;
          if ( distance < _avoidDistance ) avoidVector -= otherPosition - GlobalPosition ;
        }
      }

      if ( otherCount > 0 ) {
        alignVector /= otherCount ;
        flockCenter /= otherCount ;
        centerVector = GlobalPosition.DirectionTo( flockCenter ) ;
      }

      return ( centerVector, alignVector, avoidVector, otherCount ) ;
    }

    public void AddTarget( Vector2 targetPosition )
    {
      _targets.Add( targetPosition ) ;
    }

    public void ClearTarget()
    {
      _targets.Clear() ;
    }
  }
}