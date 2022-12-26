using Godot ;

namespace Fish.Utilities
{
  public static class GraphicsExtensions
  {
    public static Vector2 GameWorldScreenSize = new Vector2( 240, 120 ) ;
    public const float ScreenSizeScaleFactor = 20 ; // with distance of camera = 20, fov = 70, this value is guess from trial and error method :)

    public static void Flip( this Vector3 moveDirection, Spatial graphics, float raisedDegreesX, float raisedDegreesZ )
    {
      const float flipDegrees = 90f ;
      const float weight = 0.15f ;
      const float rotationThreshold = 0.3f ;
      var rotationDegreesY = moveDirection.x > 0 ? flipDegrees : moveDirection.x < 0 ? -flipDegrees : graphics.RotationDegrees.y ;
      // This can be simplified to just the X axis, but I added the Z axis to illustrate 2.5D movement (better visual)
      var rotationDegreesX = moveDirection.y > rotationThreshold ? -raisedDegreesX : moveDirection.y < -rotationThreshold ? raisedDegreesX : 0 * moveDirection.y ;
      var rotationDegreesZ = moveDirection.y > rotationThreshold ? -raisedDegreesZ : moveDirection.y < -rotationThreshold ? raisedDegreesZ : 0 * moveDirection.y ;
      var lerpX = Mathf.Lerp( graphics.RotationDegrees.x, rotationDegreesX, weight ) ;
      var lerpY = Mathf.Lerp( graphics.RotationDegrees.y, rotationDegreesY, weight ) ;
      var lerpZ = Mathf.Lerp( graphics.RotationDegrees.z, rotationDegreesZ, weight ) ;
      graphics.RotationDegrees = new Vector3( lerpX, lerpY, lerpZ ) ;
    }
  }
}