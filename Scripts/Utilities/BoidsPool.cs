using System.Collections.Generic ;
using System.Diagnostics ;
using System.Linq ;
using Fish.Scripts.Nodes ;
using Godot ;

namespace Fish.Scripts.Utilities
{
  public class BoidsPool : Spatial
  {
    public class BoidsPoolParameter
    {
      public int StartingBoidsCount { get ; set ; }
      public PackedScene BoidScene { get ; set ; }
      public string BoidGroupName { get ; set ; }
      public IGridStructure GridStructure { get ; set ; }
      public Vector2 ScreenSize { get ; set ; }
      public Node BoidsNode { get ; set ; }
      public Spatial Owner { get ; set ; }
      public RID Space { get ; set ; }
    }

    private BoidsPoolParameter _parameter ;
    private readonly List<Boid> _pool = new List<Boid>() ;

    public void InitPool( BoidsPoolParameter parameter )
    {
      _parameter = parameter ;
      for ( var i = 0 ; i < _parameter.StartingBoidsCount ; i++ ) {
        GD.Randomize() ;
        if ( ! ( _parameter.BoidScene.Instance() is Boid boid ) ) return ;
        // Add padding 1 to avoid checking out of bound error
        var initialPosition = new Vector3( GD.Randf() * ( _parameter.ScreenSize.x - 1 ), GD.Randf() * ( _parameter.ScreenSize.y - 1 ), 0 ) + Vector3.One ;
        boid.Translation = initialPosition ;
        boid.ScaledPoint = initialPosition.ToVector2() ;
        boid.AddToGroup( _parameter.BoidGroupName ) ;
        _pool.Add( boid ) ;
        _parameter.BoidsNode.AddChild( boid ) ;
        SetupPhysicsServer( _parameter.Space, initialPosition ) ;
      }
    }

    public void UpdateGridStructure()
    {
      _parameter.GridStructure.ClearData() ;
      _pool.AsParallel().ForAll( boid => _parameter.GridStructure.UpdateBodyPosition( boid, _parameter.GridStructure.ScalePoint( boid.Translation.ToVector2() ) ) ) ;
      _pool.AsParallel().ForAll( boid => boid.Flock = _parameter.GridStructure.GetBodiesAround( _parameter.GridStructure.ScalePoint( boid.Translation.ToVector2() ) ) ) ;
      // _pool.ForEach( boid => _parameter.GridStructure.UpdateBodyPosition( boid, _parameter.GridStructure.ScalePoint( boid.Translation.ToVector2() ) ) ) ;
      // _pool.ForEach( boid => boid.Flock = _parameter.GridStructure.GetBodiesAround( _parameter.GridStructure.ScalePoint( boid.Translation.ToVector2() ) ) ) ;
    }

    public void PhysicsProcess( float delta )
    {
      for ( var index = 0 ; index < _pool.Count ; index++ ) {
        var body = _bodies[ index ] ;
        _pool[ index ].Translation += Boid.CalculateLinearVelocity( _pool[ index ] ) * delta ;
        PhysicsServer.BodySetState( body, PhysicsServer.BodyState.Transform, new Transform( Basis.Identity, _pool[ index ].Translation ) ) ;
      }

      base._Process( delta ) ;
    }

    private RID _bodyRid ;
    private RID _shapeRid ;
    private List<RID> _bodies = new List<RID>() ;

    private Dictionary<string, float> dict = new Dictionary<string, float>
    {
      { "radius", 1.0f },
      { "height", 1.0f },
    } ;

    private void SetupPhysicsServer( RID space, Vector3 origin )
    {
      // var list = new List<Transform>( num ) ;
      _bodyRid = PhysicsServer.BodyCreate( PhysicsServer.BodyMode.Kinematic ) ;
      _shapeRid = PhysicsServer.ShapeCreate( PhysicsServer.ShapeType.Capsule ) ;
      _bodies.Add( _bodyRid ) ;
      PhysicsServer.ShapeSetData( _shapeRid, dict ) ;
      PhysicsServer.BodyAddShape( _bodyRid, _shapeRid ) ;
      PhysicsServer.BodySetSpace( _bodyRid, space ) ;
      var xForm = new Transform( Basis.Identity, origin ) ;
      // list.Add( xForm ) ;
      PhysicsServer.BodySetState( _bodyRid, PhysicsServer.BodyState.Transform, xForm ) ;
      // return list ;
    }
  }
}