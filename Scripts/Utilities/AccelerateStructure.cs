using System ;
using System.Collections.Concurrent ;
using System.Collections.Generic ;
using System.Diagnostics ;
using System.Linq ;
using System.Runtime.CompilerServices ;
using Fish.Scripts.Nodes ;
using Godot ;

namespace Fish.Scripts.Utilities
{
  public interface ICell
  {
    public Vector2 ScaledPoint { get ; set ; }
  }

  public interface IGridStructure
  {
    public Vector2 ScalePoint( Vector2 unscalePoint ) ;
    public void UpdateBodyPosition( Boid body, Vector2 unscaledPoint ) ;
    public HashSet<Boid> GetBodiesAround( Vector2 unscaledPoint ) ;
    public void ClearData() ;
  }

  public class BoidAccelerateStructure2D : IGridStructure
  {
    private readonly Vector2 _gridSize ;

    private readonly int _scaleDownFactor ;

    private readonly HashSet<Boid>[ , ] _cachedCells ;

    public BoidAccelerateStructure2D( Vector2 unscaleGridSize, int scaleDownFactor )
    {
      _scaleDownFactor = scaleDownFactor ;
      _gridSize = ( unscaleGridSize / _scaleDownFactor ).Floor() ;
      _cachedCells = new HashSet<Boid>[ (int) _gridSize.x + 1, (int) _gridSize.y + 1 ] ;
      for ( var row = 0 ; row < _cachedCells.GetLength( 0 ) ; row++ ) {
        for ( var col = 0 ; col < _cachedCells.GetLength( 1 ) ; col++ ) {
          _cachedCells[ row, col ] = new HashSet<Boid>() ;
        }
      }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Vector2 ScalePoint( Vector2 unscalePoint )
    {
      var scalePoint = ( unscalePoint / _scaleDownFactor ).Floor() ;
      // Add padding to avoid sanity check -> improve performance
      return scalePoint.Clamp( Vector2.One, _gridSize - Vector2.One ) ;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private HashSet<Boid> GetCachedBodies( Vector2 scaledPoint ) => _cachedCells[ (int) scaledPoint.x, (int) scaledPoint.y ] ;

    public void UpdateBodyPosition( Boid body, Vector2 unscaledPoint )
    {
      const int max = 15 ;
      var oldScaledPosition = body.ScaledPoint ;
      var scaledPoint = ScalePoint( unscaledPoint ) ;
      body.ScaledPoint = scaledPoint ;
      if ( scaledPoint == Vector2.Zero || scaledPoint == _gridSize ) return ;
      try {
        GetCachedBodies( scaledPoint ).AddThreadSafe( body ) ;
        var movingDirection = ( scaledPoint - oldScaledPosition ).Normalized() ;
        var (movingRight, movingLeft, movingDown, movingUp) = body.IsInsideTree() ? ( movingDirection.x > 0, movingDirection.x < 0, movingDirection.y < 0, movingDirection.y > 0 ) : ( true, true, true, true ) ;
        if ( movingLeft ) {
          GetCachedBodies( scaledPoint + Vector2.Left ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Left - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Left - Vector2.Down ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Down ).AddThreadSafe( body ) ;
        }
        else if ( movingRight ) {
          GetCachedBodies( scaledPoint + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Right - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Right - Vector2.Down ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Down ).AddThreadSafe( body ) ;
        }

        if ( movingUp ) {
          GetCachedBodies( scaledPoint - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Up + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Up + Vector2.Left ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Left ).AddThreadSafe( body ) ;
        }
        else if ( movingDown ) {
          GetCachedBodies( scaledPoint - Vector2.Down ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Down + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint - Vector2.Down + Vector2.Left ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( scaledPoint + Vector2.Left ).AddThreadSafe( body ) ;
        }
      }
      catch ( Exception ex ) {
        GD.PrintErr( ex.ToString() ) ;
      }
    }

    public HashSet<Boid> GetBodiesAround( Vector2 unscaledPoint )
    {
      return GetCachedBodies( ScalePoint( unscaledPoint ) ) ;
    }

    public void ClearData()
    {
      for ( var row = 0 ; row < _cachedCells.GetLength( 0 ) ; row++ ) {
        for ( var col = 0 ; col < _cachedCells.GetLength( 1 ) ; col++ ) {
          _cachedCells[ row, col ] = new HashSet<Boid>() ;
        }
      }
    }
  }
}