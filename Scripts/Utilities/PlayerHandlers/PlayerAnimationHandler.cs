using Godot ;

namespace Fish.Scripts.Utilities.PlayerHandlers
{
  class PlayerAnimationHandler
  {
    private const string AnimationPlayerPath = "Graphics/AnimationPlayer" ;
    private const string AnimationSwimmingFastName = "Swimming_Fast" ;
    private const string AnimationSwimmingImpulseName = "Attack" ;

    private readonly AnimationPlayer _animationPlayer ;

    public PlayerAnimationHandler( Node owner )
    {
      _animationPlayer = owner.GetNode<AnimationPlayer>( AnimationPlayerPath ) ;
      _animationPlayer.Play( AnimationSwimmingFastName ) ;
      _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
    }

    public void UpdateAnimation( Vector3 moveDirection, bool isDashing )
    {
      // To simulate that fish swims faster when moving
      _animationPlayer.PlaybackSpeed = moveDirection == Vector3.Zero ? 1f : 1.7f ;
      if ( isDashing ) _animationPlayer.Play( AnimationSwimmingImpulseName ) ;
      if ( _animationPlayer.IsPlaying() ) return ;
      _animationPlayer.Play( AnimationSwimmingFastName ) ;
      _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
    }
  }
}