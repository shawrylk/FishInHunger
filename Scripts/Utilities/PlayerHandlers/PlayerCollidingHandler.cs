using System.ComponentModel ;
using System.Data ;
using Fish.Scripts.Utilities.FoodChain ;
using Godot ;

namespace Fish.Scripts.Utilities.PlayerHandlers
{
  class PlayerCollidingHandler : GeneratorBase
  {
    private readonly KinematicBody _owner ;
    // private FoodChainHandler _foodChainHandler ;

    public PlayerCollidingHandler( KinematicBody owner )
    {
      _owner = owner ;
      if ( ! ( _owner is IFood food ) ) throw new InvalidConstraintException( $"Owner does not implement interface {nameof( IFood )}" ) ;
      // _foodChainHandler = new FoodChainHandler( food ) ;
    }

    // private void RestartGame()
    // {
    //   _owner.GetTree().ReloadCurrentScene() ;
    // }

    // private int FishEatenCount = 0 ;

    public void HandleCollider()
    {
      for ( int index = 0, count = _owner.GetSlideCount() ; index < count ; index++ ) {
        var collision = _owner.GetSlideCollision( index ) ;
        // if ( _foodChainHandler.HandleCollision( collision ) ) return ;
        AddNewWork( collision ) ;
      }
    }
  }
}