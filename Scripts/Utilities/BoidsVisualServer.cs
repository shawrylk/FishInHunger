using System.Collections.Generic ;
using Godot ;

namespace Fish.Scripts.Utilities
{
  public class BoidsVisualServer : Spatial
  {
    private Mesh _boidMesh ;
    private KinematicBody _body ;
    private CollisionShape _shape ;
    private RID _bodyRid ;
    private RID _shapeRid ;
    private List<RID> _bodies ;
    private List<RID> _visuals ;
    private List<Vector3> _origins ;
    private const string BoidMeshPath = "res://Models/ZebraClownFish/ZebraClownFish.obj" ;
    private Basis _basisY90 = new Basis( new Vector3( 0, 0, -1 ), new Vector3( 0, 1, 0 ), new Vector3( 1, 0, 0 ) ) ;

    public override void _Ready()
    {
      var num = 1 ;
      _bodies = new List<RID>( num ) ;
      _visuals = new List<RID>( num ) ;
      _origins = new List<Vector3>( num ) ;
      var list = SetupPhysicsServer( num ) ;
      // SetupVisualServer( list ) ;
      base._Ready() ;
    }

    public override void _PhysicsProcess( float delta )
    {
      // for ( var index = 0 ; index < _bodies.Count ; index++ ) {
      //   var body = _bodies[ index ] ;
      //   _origins[ index ] += Vector2.One.ToVector3( 0 ) * delta * 1 ;
      //   PhysicsServer.BodySetState( body, PhysicsServer.BodyState.Transform, new Transform( Basis.Identity, _origins[ index ] ) ) ;
      //   VisualServer.InstanceSetTransform( _visuals[ index ], new Transform( _basisY90, _origins[ index ] ) ) ;
      // }

      base._Process( delta ) ;
    }

    // private void SetupVisualServer( List<Transform> transforms )
    // {
    //   var scenario = GetWorld().Scenario ;
    //   _boidMesh = GD.Load( BoidMeshPath ) as Mesh ;
    //   if ( _boidMesh is null ) return ;
    //   for ( var i = 0 ; i < transforms.Count ; i++ ) {
    //     var instance = VisualServer.InstanceCreate() ;
    //     _visuals.Add( instance ) ;
    //     VisualServer.InstanceSetScenario( instance, scenario ) ;
    //     VisualServer.InstanceSetBase( instance, _boidMesh.GetRid() ) ;
    //     GD.Randomize() ;
    //     var xForm = new Transform( _basisY90, transforms[ i ].origin ) ;
    //     VisualServer.InstanceSetTransform( instance, xForm ) ;
    //   }
    // }

    private List<Transform> SetupPhysicsServer( int num )
    {
      var list = new List<Transform>( num ) ;
      var space = GetWorld().Space ;
      var dict = new Dictionary<string, float>
      {
        { "radius", 1.0f },
        { "height", 1.0f },
      } ;
      for ( var i = 0 ; i < num ; i++ ) {
        _bodyRid = PhysicsServer.BodyCreate( PhysicsServer.BodyMode.Kinematic ) ;
        _shapeRid = PhysicsServer.ShapeCreate( PhysicsServer.ShapeType.Capsule ) ;
        _bodies.Add( _bodyRid ) ;
        PhysicsServer.ShapeSetData( _shapeRid, dict ) ;
        PhysicsServer.BodyAddShape( _bodyRid, _shapeRid ) ;
        PhysicsServer.BodySetSpace( _bodyRid, space ) ;
        var xForm = new Transform( Basis.Identity, new Vector3( ( GD.Randf() + 1 ) * 120, ( GD.Randf() + 1 ) * 60, 0 ) ) ;
        list.Add( xForm ) ;
        _origins.Add( xForm.origin ) ;
        PhysicsServer.BodySetState( _bodyRid, PhysicsServer.BodyState.Transform, xForm ) ;
      }

      return list ;
    }
  }
}