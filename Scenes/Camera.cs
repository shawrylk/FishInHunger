using Godot ;
using System ;
using System.Collections.Generic ;
using Fish.Utilities ;

public class Camera : Godot.Camera
{
  private const string FollowTargetPath = "%Player" ;
  private Spatial _followTarget ;
  private Vector3 _halfExtent ;
  private Vector3 _limit ;

  [Export]
  private Vector3 _distance ;

  public override void _Ready()
  {
    _followTarget = GetNode<Spatial>( FollowTargetPath ) ;
    _halfExtent = ( GetViewport().Size / 2 / GraphicsExtensions.ScreenSizeScaleFactor ).ToVector3( 0 ) ;
    _limit = GraphicsExtensions.GameWorldScreenSize.ToVector3( _distance.z ) ;
  }

  public override void _PhysicsProcess( float delta )
  {
    var toTranslation = ( _followTarget.Translation + _distance ).Clamp( _halfExtent, _limit - _halfExtent ) ;
    Translation = Translation.LinearInterpolate( toTranslation, 0.5f ) ;
  }
}