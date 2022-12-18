using Godot ;

namespace Fish.Utilities
{
  public static class VectorExtensions
  {
    public static (float x, float y) Deconstruct( this Vector2 vector2 ) => ( vector2.x, vector2.y ) ;
  }
}