using Godot ;

namespace Fish.Utilities
{
  public static class GraphicsExtensions
  {
    public static void Flip( this Vector3 moveDirection, Spatial graphics, float raisedDegrees )
    {
      const float flipDegrees = 90f ;
      const float weight = 0.15f ;
      var rotationDegreesY = moveDirection.x > 0 ? flipDegrees : moveDirection.x < 0 ? -flipDegrees : graphics.RotationDegrees.y ;
      // This can be simplified to just the X axis, but I added the Z axis to illustrate 2.5D movement (better visual)
      var rotationDegreesXnZ = moveDirection.y > 0 ? -raisedDegrees : moveDirection.y < 0 ? raisedDegrees : 0 ;
      var lerpX = Mathf.Lerp( graphics.RotationDegrees.x, rotationDegreesXnZ, weight ) ;
      var lerpY = Mathf.Lerp( graphics.RotationDegrees.y, rotationDegreesY, weight ) ;
      var lerpZ = Mathf.Lerp( graphics.RotationDegrees.z, rotationDegreesXnZ, weight ) ;
      graphics.RotationDegrees = new Vector3( lerpX, lerpY, lerpZ ) ;
    }
  }
}