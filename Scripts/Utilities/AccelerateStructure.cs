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
      if ( scaledPoint == oldScaledPosition || scaledPoint == Vector2.Zero || scaledPoint == _gridSize ) return ;
      try {
        // Add (Notice: Y axis is inverted)
        // var (validMinX, validMaxX, validMinY, validMaxY) = ( oldScaledPosition.x > 0, oldScaledPosition.x < _gridSize.x, oldScaledPosition.y > 0, oldScaledPosition.y < _gridSize.y ) ;

        // GetCachedBodies( oldScaledPosition ).RemoveThreadSafe( body ) ;
        // if ( validMinX ) GetCachedBodies( oldScaledPosition + Vector2.Left ).RemoveThreadSafe( body ) ;
        // if ( validMaxX ) GetCachedBodies( oldScaledPosition + Vector2.Right ).RemoveThreadSafe( body ) ;
        // if ( validMinY ) GetCachedBodies( oldScaledPosition - Vector2.Down ).RemoveThreadSafe( body ) ;
        // if ( validMaxY ) GetCachedBodies( oldScaledPosition - Vector2.Up ).RemoveThreadSafe( body ) ;
        // if ( validMinX && validMinY ) GetCachedBodies( oldScaledPosition + Vector2.Left - Vector2.Down ).RemoveThreadSafe( body ) ;
        // if ( validMinX && validMaxY ) GetCachedBodies( oldScaledPosition + Vector2.Left - Vector2.Up ).RemoveThreadSafe( body ) ;
        // if ( validMaxX && validMinY ) GetCachedBodies( oldScaledPosition + Vector2.Right - Vector2.Down ).RemoveThreadSafe( body ) ;
        // if ( validMaxX && validMaxY ) GetCachedBodies( oldScaledPosition + Vector2.Right - Vector2.Up ).RemoveThreadSafe( body ) ;

        // ( validMinX, validMaxX, validMinY, validMaxY ) = ( scaledPoint.x > 0, scaledPoint.x < _gridSize.x, scaledPoint.y > 0, scaledPoint.y < _gridSize.y ) ;
        // GetCachedBodies( scaledPoint ).AddThreadSafe( body, max ) ;
        // if ( validMinX ) GetCachedBodies( scaledPoint + Vector2.Left ).AddThreadSafe( body, max ) ;
        // if ( validMaxX ) GetCachedBodies( scaledPoint + Vector2.Right ).AddThreadSafe( body, max ) ;
        // if ( validMinY ) GetCachedBodies( scaledPoint - Vector2.Down ).AddThreadSafe( body, max ) ;
        // if ( validMaxY ) GetCachedBodies( scaledPoint - Vector2.Up ).AddThreadSafe( body, max ) ;
        // if ( validMinX && validMinY ) GetCachedBodies( scaledPoint + Vector2.Left - Vector2.Down ).AddThreadSafe( body, max ) ;
        // if ( validMinX && validMaxY ) GetCachedBodies( scaledPoint + Vector2.Left - Vector2.Up ).AddThreadSafe( body, max ) ;
        // if ( validMaxX && validMinY ) GetCachedBodies( scaledPoint + Vector2.Right - Vector2.Down ).AddThreadSafe( body, max ) ;
        // if ( validMaxX && validMaxY ) GetCachedBodies( scaledPoint + Vector2.Right - Vector2.Up ).AddThreadSafe( body, max ) ;
        // Remove (Notice: Y axis is inverted)
        // if ( oldScaledPosition == Vector2.Zero && body.IsInsideTree() ) return ;
        var movingDirection = ( scaledPoint - oldScaledPosition ).Normalized() ;
        var (movingRight, movingLeft, movingDown, movingUp) = body.IsInsideTree() ? ( movingDirection.x > 0, movingDirection.x < 0, movingDirection.y < 0, movingDirection.y > 0 ) : ( true, true, true, true ) ;
        if ( movingLeft ) {
          GetCachedBodies( oldScaledPosition + Vector2.Left ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition + Vector2.Left - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition + Vector2.Left - Vector2.Down ).AddThreadSafe( body ) ;
        }
        else if ( movingRight ) {
          GetCachedBodies( oldScaledPosition + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition + Vector2.Right - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition + Vector2.Right - Vector2.Down ).AddThreadSafe( body ) ;
        }

        if ( movingUp ) {
          GetCachedBodies( oldScaledPosition - Vector2.Up ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition - Vector2.Up + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition - Vector2.Up + Vector2.Left ).AddThreadSafe( body ) ;
        }
        else if ( movingDown ) {
          GetCachedBodies( oldScaledPosition - Vector2.Down ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition - Vector2.Down + Vector2.Right ).AddThreadSafe( body ) ;
          GetCachedBodies( oldScaledPosition - Vector2.Down + Vector2.Left ).AddThreadSafe( body ) ;
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