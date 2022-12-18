using System.Collections.Generic ;
using System.Linq ;
using System.Runtime.CompilerServices ;
using Godot ;

namespace Fish.Utilities
{
  public class AccelerateStructure
  {
    private readonly Vector2 _gridSize ;
    private readonly int _scaleDownFactor ;
    private readonly List<Node2D>[ , ] _cells ;

    public AccelerateStructure( Vector2 unscaleGridSize, int scaleDownFactor )
    {
      _scaleDownFactor = scaleDownFactor ;
      _gridSize = ( unscaleGridSize / _scaleDownFactor ).Floor() ;
      _cells = new List<Node2D>[ (int) _gridSize.x + 1, (int) _gridSize.y + 1 ] ;
      for ( var row = 0 ; row < _cells.GetLength( 0 ) ; row++ ) {
        for ( var col = 0 ; col < _cells.GetLength( 1 ) ; col++ ) {
          _cells[ row, col ] = new List<Node2D>() ;
        }
      }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Vector2 ScalePoint( Vector2 unscalePoint )
    {
      var scalePoint = ( unscalePoint / _scaleDownFactor ).Floor() ;
      scalePoint.x = Mathf.Clamp( scalePoint.x, 0, _gridSize.x ) ;
      scalePoint.y = Mathf.Clamp( scalePoint.y, 0, _gridSize.y ) ;
      return scalePoint ;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private List<Node2D> GetBodies( Vector2 scaledPoint ) => _cells[ (int) scaledPoint.x, (int) scaledPoint.y ] ;

    public void AddBody( Node2D body, Vector2 unscaledPoint )
    {
      var scaledPoint = ScalePoint( unscaledPoint ) ;
      GetBodies( scaledPoint ).Add( body ) ;
    }

    public List<List<Node2D>> GetBodiesAround( Vector2 unscaledPoint )
    {
      var scaledPoint = ScalePoint( unscaledPoint ) ;
      return new HashSet<List<Node2D>>
      {
        GetBodies( scaledPoint ),
        GetBodies( scaledPoint + Vector2.Up ),
        GetBodies( scaledPoint + Vector2.Up + Vector2.Left ),
        GetBodies( scaledPoint + Vector2.Up + Vector2.Right ),
        GetBodies( scaledPoint + Vector2.Down ),
        GetBodies( scaledPoint + Vector2.Down + Vector2.Left ),
        GetBodies( scaledPoint + Vector2.Down + Vector2.Right ),
        GetBodies( scaledPoint + Vector2.Left ),
        GetBodies( scaledPoint + Vector2.Right ),
      }.ToList() ;
    }
  }
}