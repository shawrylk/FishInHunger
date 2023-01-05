using System.Collections.Generic ;
using System.Threading.Tasks ;
using Fish.Scripts.Utilities ;
using Godot ;

namespace Fish.Scripts.Nodes
{
  public class RandomSpawn : Spatial
  {
    [Export]
    private int _startingBoidsCount = 100 ;

    [Export]
    private PackedScene _boidScene = GD.Load( "res://Scenes/RedSnapper.tscn" ) as PackedScene ;

    [Export]
    private int _computeGroup = 4 ;

    private const string BoidsGroupName = "Boids" ;
    private const string BoidsNodePath = "Boids" ;
    private BoidAccelerateStructure2D _accelStruct ;
    private Node _boidsNode ;
    private Vector2 _screenSize ;
    private readonly List<Boid> _boids = new List<Boid>() ;

    public override void _Ready()
    {
      _screenSize = GraphicsExtensions.GameWorldScreenSize ;
<<<<<<< Updated upstream
      // _screenSize = GetViewport().Size ;
      _boidsNode = GetParent().GetNode<Node>( BoidsNodePath ) ;
      InitBoids() ;
=======
      _boidsPool.InitPool( new BoidsPool.BoidsPoolParameter
      {
        Space = GetWorld().Space,
        BoidGroupName = BoidsGroupName,
        ScreenSize = _screenSize,
        BoidsNode = GetParent().GetNode<Node>( BoidsGroupNodePath ),
        BoidScene = _boidScene,
        StartingBoidsCount = _startingBoidsCount,
        GridStructure = new BoidAccelerateStructure2D( _screenSize, 1 ),
      } ) ;
>>>>>>> Stashed changes
      base._Ready() ;
    }

    private void InitBoids()
    {
      for ( var i = 0 ; i < _startingBoidsCount ; i++ ) {
        GD.Randomize() ;
        if ( ! ( _boidScene.Instance() is Boid boid ) ) return ;
        var initialPosition = new Vector3( GD.Randf() * _screenSize.x, GD.Randf() * _screenSize.y, 0 ) ;
        boid.Translation = initialPosition ;
        boid.AddToGroup( BoidsGroupName ) ;
        _boids.Add( boid ) ;
        _boidsNode.AddChild( boid ) ;
      }
    }

    public override void _Process( float delta )
    {
<<<<<<< Updated upstream
      var scalePoints = BuildStruct() ;
      UpdateBoids( scalePoints ) ;
      ProcessBoids( delta ) ;
      base._Process( delta ) ;
    }

    private List<Vector2> BuildStruct()
    {
      var structScale = (int) ( _boids[ 0 ].ViewDistance / 2 ) ;
      var scaledPoints = new List<Vector2>( _boids.Count ) ;
      _accelStruct = new BoidAccelerateStructure2D( _screenSize, structScale ) ;
      foreach ( var boid in _boids ) {
        var scaledPoint = _accelStruct.ScalePoint( boid.Translation.ToVector2() ) ;
        _accelStruct.AddBody( boid, scaledPoint ) ;
        scaledPoints.Add( scaledPoint ) ;
      }

      return scaledPoints ;
    }

    private void UpdateBoids( List<Vector2> scaledPoints )
    {
      for ( var i = 0 ; i < _boids.Count ; i++ ) _boids[ i ].Flock = _accelStruct.GetBodiesAround( scaledPoints[ i ] ) ;
    }

    private void ProcessBoids( float delta )
    {
      for ( var groupIndex = 0 ; groupIndex < _computeGroup ; groupIndex++ ) {
        var index = groupIndex ;
        Task.Run( () => { ProcessGroup( delta, index ) ; } ) ;
      }
    }

    private void ProcessGroup( float delta, int groupIndex )
    {
      var start = _boids.Count / _computeGroup * groupIndex ;
      var end = start + _boids.Count / _computeGroup + ( groupIndex + 1 == _boids.Count ? _boids.Count % _computeGroup : 0 ) ;
      for ( var i = start ; i < end ; i++ ) _boids[ i ]._Process( delta ) ;
=======
      // using var _ = DebugUtilities.StartDisposableStopwatch() ;
      _boidsPool.UpdateGridStructure() ;
      base._Process( delta ) ;
    }

    public override void _PhysicsProcess( float delta )
    {
      _boidsPool.PhysicsProcess( delta ) ;
      base._PhysicsProcess( delta ) ;
>>>>>>> Stashed changes
    }
  }
}