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
    private int _startingBoidsCount = 300 ;

    [Export]
    private PackedScene _boidScene = GD.Load( "res://Scenes/Boid.tscn" ) as PackedScene ;

    private const string BoidsGroupName = "Boids" ;
    public const string BoidsGroupNodePath = "Boids" ;
    public const string GridMapNodePath = "GridMap" ;
    private Vector2 _screenSize ;
    private readonly BoidsPool _boidsPool = new BoidsPool() ;

    public override void _Ready()
    {
      _screenSize = GraphicsExtensions.GameWorldScreenSize ;
      var gridMap = GetParent().GetNode<GridMap>( GridMapNodePath ) ;
      var cellSize = gridMap.CellSize ;
      var tileSize = 2f ;

      _boidsPool.InitPool( new BoidsPool.BoidsPoolParameter
      {
        BoidGroupName = BoidsGroupName,
        ScreenSize = _screenSize,
        BoidsNode = GetParent().GetNode<Node>( BoidsGroupNodePath ),
        BoidScene = _boidScene,
        StartingBoidsCount = _startingBoidsCount,
        GridStructure = new BoidAccelerateStructure2D( _screenSize, 4 ),
        CollidingCells = new BvhStructure( gridMap?.GetUsedCells().OfType<Vector3>().Select( vector => new BoundingBox( vector * tileSize, ( vector + cellSize ) * tileSize ) ).ToList() )
      } ) ;
      base._Ready() ;
    }

    public override void _Process( float delta )
    {
      // using var _ = DebugUtilities.StartDisposableStopwatch() ;
      _boidsPool.UpdateGridStructure() ;
      base._Process( delta ) ;
    }
  }
}