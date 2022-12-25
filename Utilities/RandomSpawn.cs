using System.Collections.Generic ;
using System.Threading.Tasks ;
using Godot ;

namespace Fish.Utilities
{
  public class RandomSpawn : Node
  {
    [Export]
    private int _startingBoidsCount = 500 ;

    [Export]
    private PackedScene _boidScene ;

    [Export]
    private int _computeGroup = 4 ;

    private Vector2 _screenSize ;
    private BoidAccelerateStructure2D _accelStruct ;
    private readonly List<Boid> _boids = new List<Boid>() ;
    private Node _boidsNode ;
    private GridMap _gridNode ;
    private const string BoidsGroupName = "Boids" ;
    private const string BoidsNodePath = "Boids" ;
    private const string GridMapPath = "Grid" ;

    public override void _Ready()
    {
      _screenSize = GetViewport().Size ;
      _boidsNode = GetParent().GetNode<Node>( BoidsNodePath ) ;
      _gridNode = GetNode<GridMap>( GridMapPath ) ;
      InitBoids() ;
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
      var scalePoints = BuildStruct() ;
      UpdateBoids( scalePoints ) ;
      // _gridNode.Scale = _accelStruct.GridSize ;
      // _gridNode.siz
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

      // Parallel.ForEach( Enumerable.Range( 0, _computeGroup ).ToList(), index => ProcessGroup( delta, index ) ) ;
    }

    private void ProcessGroup( float delta, int groupIndex )
    {
      var start = _boids.Count / _computeGroup * groupIndex ;
      var end = start + _boids.Count / _computeGroup + ( groupIndex + 1 == _boids.Count ? _boids.Count % _computeGroup : 0 ) ;
      for ( var i = start ; i < end ; i++ ) _boids[ i ]._Process( delta ) ;
      // Parallel.ForEach( Enumerable.Range( start, end ).ToList(), index => _boids[ index ]._Process( delta ) ) ;
    }
  }
}