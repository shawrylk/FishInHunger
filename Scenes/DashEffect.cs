using Godot ;
using System ;

public class DashEffect : Sprite
{
  private const string TweenPath = "Tween" ;
  private const string InterpolateProperty = "modulate:a" ;
  private Tween _tween ;

  public override void _Ready()
  {
    _tween = GetNode<Tween>( TweenPath ) ;
    _tween.InterpolateProperty( this, InterpolateProperty, 1f, 0f, .5f, Tween.TransitionType.Quart, Tween.EaseType.In ) ;
    _tween.Start() ;
    base._Ready() ;
  }
}