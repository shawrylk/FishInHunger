using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;
using Fish.Scripts.Utilities ;
using Godot ;

namespace Fish.Scripts.Nodes
{
  public class RandomSpawn : Node
  {
    [Export]
    private int _startingBoidsCount = 100 ;

    [Export]
    private PackedScene _boidScene = GD.Load( "res://Scenes/Boid.tscn" ) as PackedScene ;

    [Export]
    private int _startingBoids2Count = 10 ;

    [Export]
    private PackedScene _boid2Scene = GD.Load( "res://Scenes/Boid2.tscn" ) as PackedScene ;

    private const string BoidsGroupName = "Boids" ;
    public const string BoidsGroupNodePath = "Boids" ;
    public const string GridMapNodePath = "GridMap" ;
    private Vector2 _screenSize ;
    private readonly BoidsPool _boidsPool = new BoidsPool() ;
    public BoidsPool BoidsPool => _boidsPool ;
    private readonly BoidsPool _boids2Pool = new BoidsPool() ;
    public BoidsPool Boids2Pool => _boids2Pool ;

    public override void _Ready()
    {
      _screenSize = GraphicsExtensions.GameWorldScreenSize ;
      var gridMap = GetParent().GetNode<GridMap>( GridMapNodePath ) ;
      var cellSize = gridMap.CellSize ;
      var tileSize = 2f ;
      var collidingCells = new BvhStructure( gridMap?.GetUsedCells().OfType<Vector3>().Select( vector => new BoundingBox( vector * tileSize, ( vector + cellSize ) * tileSize ) ).ToList() ) ;
      _boidsPool.InitPool( new BoidsPool.BoidsPoolParameter
      {
        BoidGroupName = BoidsGroupName,
        ScreenSize = _screenSize,
        BoidsNode = GetParent().GetNode<Node>( BoidsGroupNodePath ),
        BoidScene = _boidScene,
        StartingBoidsCount = _startingBoidsCount,
        GridStructure = new BoidAccelerateStructure2D( _screenSize, 4 ),
        CollidingCells = collidingCells
      } ) ;

      _boids2Pool.InitPool( new BoidsPool.BoidsPoolParameter
      {
        BoidGroupName = BoidsGroupName,
        ScreenSize = _screenSize,
        BoidsNode = GetParent().GetNode<Node>( BoidsGroupNodePath ),
        BoidScene = _boid2Scene,
        StartingBoidsCount = _startingBoids2Count,
        GridStructure = new BoidAccelerateStructure2D( _screenSize, 4 ),
        CollidingCells = collidingCells
      } ) ;
      base._Ready() ;
    }

    public override void _Process( float delta )
    {
      // using var _ = DebugUtilities.StartDisposableStopwatch() ;
      _boidsPool.UpdateGridStructure() ;
      _boids2Pool.UpdateGridStructure() ;
      base._Process( delta ) ;
    }
  }
}