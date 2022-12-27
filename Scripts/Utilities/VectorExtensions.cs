using Godot ;

namespace Fish.Scripts.Utilities
{
  public static class VectorExtensions
  {
    public static (float x, float y) Deconstruct( this Vector2 vector2 ) => ( vector2.x, vector2.y ) ;
    public static Vector2 ToVector2( this Vector3 vector3 ) => new Vector2( vector3.x, vector3.y ) ;
    public static Vector3 ToVector3( this Vector2 vector2, float z ) => new Vector3( vector2.x, vector2.y, z ) ;
    public static Vector3 Clamp( this Vector3 vector3, Vector3 min, Vector3 max ) => new Vector3( Mathf.Clamp( vector3.x, min.x, max.x ), Mathf.Clamp( vector3.y, min.y, max.y ), Mathf.Clamp( vector3.z, min.z, max.z ) ) ;
  }
}