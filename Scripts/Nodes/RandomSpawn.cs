using System.Collections.Generic ;
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
    private Vector2 _screenSize ;
    private readonly BoidsPool _boidsPool = new BoidsPool() ;

    public override void _Ready()
    {
      _screenSize = GraphicsExtensions.GameWorldScreenSize ;
      _boidsPool.InitPool( new BoidsPool.BoidsPoolParameter
      {
        BoidGroupName = BoidsGroupName,
        ScreenSize = _screenSize,
        BoidsNode = GetParent().GetNode<Node>( BoidsGroupNodePath ),
        BoidScene = _boidScene,
        StartingBoidsCount = _startingBoidsCount,
        GridStructure = new BoidAccelerateStructure2D( _screenSize, 4 ),
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