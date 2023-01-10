using System.Collections.Generic ;
using System.Diagnostics ;
using System.Linq ;
using Fish.Scripts.Nodes ;
using Godot ;

namespace Fish.Scripts.Utilities
{
  public class BoidsPool
  {
    public class BoidsPoolParameter
    {
      public int StartingBoidsCount { get ; set ; }
      public PackedScene BoidScene { get ; set ; }
      public string BoidGroupName { get ; set ; }
      public IGridStructure GridStructure { get ; set ; }
      public Vector2 ScreenSize { get ; set ; }
      public Node BoidsNode { get ; set ; }
      public BvhStructure CollidingCells { get ; set ; }
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
        boid.CollidingCells = _parameter.CollidingCells ;
        _pool.Add( boid ) ;
        _parameter.BoidsNode.AddChild( boid ) ;
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
  }
}