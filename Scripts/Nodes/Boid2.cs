using System.Collections.Generic ;
using System.Linq ;
using Fish.Scripts.Utilities ;
using Godot ;

namespace Fish.Scripts.Nodes
{
  public class Boid2 : Boid
  {
    private float _playerFollowDistance = 15f ;

    public override void _Ready()
    {
      MaxSpeed = 10 ;
      MaxFlockSize = 1 ;
      SeparationForce = 1f ;
      CohesionForce = 0.1f ;
      GD.Randomize() ;
      RaiseDegreesX = (float) GD.RandRange( 5, 15 ) ;
      GD.Randomize() ;
      RaiseDegreesZ = (float) GD.RandRange( 5, 15 ) ;

      base._Ready() ;
    }

    protected override Vector3 UpdateVelocityToPlayer()
    {
      var edgeAvoidVector = Vector3.Zero ;

      if ( _player is { } ) {
        if ( Translation.DistanceTo( _player.Translation ) > _playerFollowDistance ) return edgeAvoidVector ;
        if ( Translation.x < _player.Translation.x ) edgeAvoidVector.x = 1f ;
        else if ( Translation.x > _player.Translation.x ) edgeAvoidVector.x = -1f ;
        if ( Translation.y < _player.Translation.y ) edgeAvoidVector.y = 1f ;
        else if ( Translation.y > _player.Translation.y ) edgeAvoidVector.y = -1f ;
      }

      return edgeAvoidVector ;
    }

    public override void _PhysicsProcess( float delta )
    {
      base._PhysicsProcess( delta ) ;
      for ( int index = 0, count = GetSlideCount() ; index < count ; index++ ) {
        var collision = GetSlideCollision( index ) ;
        if ( collision.Collider is Boid boid && boid.GetType() != typeof( Boid2 ) && boid.IsInGroup( RandomSpawn.BoidsGroupNodePath ) ) {
          boid.ReturnToPool() ;
        }
      }
    }
  }
}