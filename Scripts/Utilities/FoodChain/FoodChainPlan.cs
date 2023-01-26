using System ;
using Fish.Scripts.Nodes ;
using Godot ;

namespace Fish.Scripts.Utilities.FoodChain
{
  public class FoodChainPlan
  {
    private readonly IFoodUpdater _foodPositionUpdater ;

    public FoodChainPlan( IFoodUpdater foodPositionUpdater )
    {
      _foodPositionUpdater = foodPositionUpdater ;
    }

    protected bool UpdateFoodPosition( FoodChainPosition newPosition )
    {
      _foodPositionUpdater.UpdateFoodPosition( newPosition ) ;
      return true ;
    }
  }

  public class PlayerFoodChainPlan : FoodChainPlan
  {
    private readonly Player _owner ;
    private int _count = 0 ;
    private float _scale = 1f ;
    private float _tolerance = 0.01f ;
    private const float ScaleStage1 = 1f ;
    private const float ScaleStage2 = 1.5f ;
    private const float ScaleStage3 = 2.0f ;

    public PlayerFoodChainPlan( Player owner ) : base( owner )
    {
      _owner = owner ;
    }

    public bool HandleCollision( KinematicCollision collision )
    {
      if ( ! ( collision.Collider is IFood food ) ) return false ;
      switch ( food.FoodPosition ) {
        case FoodChainPosition.RedSnapper :
          _count += 1 ;
          break ;
        case FoodChainPosition.Koi :
          _count += 2 ;
          break ;
      }

      if ( _count >= 15 && Math.Abs( _scale - ScaleStage1 ) < _tolerance ) {
        _scale = ScaleStage2 ;
        _owner.UpdateFoodPosition( FoodChainPosition.MadarinFishMedium ) ;
        _owner.Scale = new Vector3( _scale, _scale, _scale ) ;
      }
      else if ( _count >= 55 && Math.Abs( _scale - ScaleStage2 ) < _tolerance ) {
        _scale = ScaleStage3 ;
        _owner.UpdateFoodPosition( FoodChainPosition.MadarinFishMedium ) ;
        _owner.Scale = new Vector3( _scale, _scale, _scale ) ;
      }

      return true ;
    }
  }
}