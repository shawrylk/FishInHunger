using System ;
using System.Collections.Generic ;
using Godot ;

namespace Fish.Scripts.Utilities
{
  public interface IBoundingBoxGetter
  {
    BoundingBox GetBoundingBox() ;
  }

  public interface IBoundingBox
  {
    public Vector3 Min { get ; }
    public Vector3 Max { get ; }
    public Vector3 Centroid { get ; }
    public Vector3.Axis LongestAxis { get ; }
    public bool Overlaps( BoundingBox other ) ;
  }

  public class BoundingBox : IBoundingBox
  {
    public Vector3 Min { get ; private set ; }
    public Vector3 Max { get ; private set ; }
    public Vector3 Centroid { get ; private set ; }

    public Vector3.Axis LongestAxis { get ; private set ; }

    public BoundingBox( Vector3 min, Vector3 max )
    {
      UpdateBoundary( min, max ) ;
    }

    public void UpdateBoundary( Vector3 min, Vector3 max )
    {
      Min = min ;
      Max = max ;
      Centroid = ( Min + Max ) * 0.5f ;
      LongestAxis = ( Max - Min ).MaxAxis() ;
    }

    public bool Overlaps( BoundingBox other )
    {
      return Min.x <= other.Max.x && other.Min.x <= Max.x &&
             Min.y <= other.Max.y && other.Min.y <= Max.y &&
             Min.z <= other.Max.z && other.Min.z <= Max.z ;
    }
  }

  public class BvhStructure
  {
    public class BvhNode
    {
      public BoundingBox Box { get ; set ; }
      public BvhNode Left { get ; set ; }
      public BvhNode Right { get ; set ; }
    }

    public BvhNode Root { get ; }

    public BvhStructure( List<BoundingBox> boundingBoxes )
    {
      Root = Build( boundingBoxes, 0, boundingBoxes.Count - 1 ) ;
    }

    // Recursive function to build the BVH tree
    private static BvhNode Build( List<BoundingBox> boundingBoxes, int start, int end )
    {
      var node = new BvhNode() ;
      node.Box = new BoundingBox( Vector3.Inf, -Vector3.Inf ) ;
      for ( var i = start ; i <= end ; i++ ) {
        node.Box.UpdateBoundary( VectorExtensions.Min( node.Box.Min, boundingBoxes[ i ].Min ), VectorExtensions.Max( node.Box.Max, boundingBoxes[ i ].Max ) ) ;
      }

      // If there is only one triangle, this is a leaf node
      if ( start == end ) return node ;

      // Find the longest axis of the bounding box
      var axis = (int) node.Box.LongestAxis ;

      // Split the triangles into two lists
      var left = new List<BoundingBox>() ;
      var right = new List<BoundingBox>() ;
      for ( var i = start ; i <= end ; i++ ) {
        if ( boundingBoxes[ i ].Centroid[ axis ] < node.Box.Centroid[ axis ] ) {
          left.Add( boundingBoxes[ i ] ) ;
        }
        else {
          right.Add( boundingBoxes[ i ] ) ;
        }
      }

      // Recursively build the left and right children
      node.Left = Build( left, 0, left.Count - 1 ) ;
      node.Right = Build( right, 0, right.Count - 1 ) ;

      return node ;
    }

    // Returns true if the BVH overlaps with the given bounding box
    public BoundingBox Overlaps( BoundingBox box )
    {
      return Overlaps( Root, box ) ;
    }

    // Recursive function to check if the given BVHNode overlaps with the bounding box
    private BoundingBox Overlaps( BvhNode node, BoundingBox box )
    {
      if ( ! node.Box.Overlaps( box ) ) return null ;

      // If this is a leaf node, check if the ray intersects with any of the triangles
      if ( node.Left == null && node.Right == null ) return node.Box ;

      // Recursively check the left and right children
      return Overlaps( node.Left, box ) ?? Overlaps( node.Right, box ) ;
    }
  }
}