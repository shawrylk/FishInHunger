using Fish.Scripts.Nodes ;
using Godot ;

namespace Fish.Scripts.Utilities.FoodChain
{
  public enum FoodChainPosition
  {
    RedSnapper,
    MadarinFishSmall,
    Koi,
    MadarinFishMedium
  }

  public interface IFood
  {
    FoodChainPosition FoodPosition { get ; }
  }

  public interface IFoodUpdater : IFood
  {
    bool UpdateFoodPosition( FoodChainPosition position ) ;
  }

  public class FoodChainHandler
  {
    private readonly IFood _ownerFoodChainPosition ;

    public FoodChainHandler( IFood ownerFoodChainPosition )
    {
      _ownerFoodChainPosition = ownerFoodChainPosition ;
    }

    public bool HandleCollision( KinematicCollision collision )
    {
      if ( ! ( collision.Collider is IFood food ) ) return false ;
      if ( (int)_ownerFoodChainPosition.FoodPosition <= (int)food.FoodPosition ) return false ;
      if ( collision.Collider is IMortalObject mortal ) mortal.Kill() ;
      return true ;
    }
  }
}