using System ;
using System.Collections.Generic ;
using System.ComponentModel ;
using System.Linq ;
using Fish.Scripts.Utilities ;
using Fish.Scripts.Utilities.FoodChain ;
using Fish.Scripts.Utilities.PlayerHandlers ;
using Godot ;

namespace Fish.Scripts.Nodes
{
  public class Player : KinematicBody, IFoodUpdater, IMortalObject
  {
    private readonly List<PlayerInputHandler> _playerActionHandlers = new List<PlayerInputHandler> { Capacity = 3 } ;
    private PlayerMovementHandler _movementHandler ;
    private PlayerAnimationHandler _animationHandler ;
    private PlayerCollidingHandler _collidingHandler ;
    private RandomSpawn _randomSpawn ;
    private PlayerFoodChainPlan _foodPlan ;
    private FoodChainHandler _foodChainHandler ;
    public FoodChainPosition FoodPosition { get ; private set ; } = FoodChainPosition.MadarinFishSmall ;

    public override void _Ready()
    {
      /*
      Create
      */

      _foodPlan = new PlayerFoodChainPlan( this ) ;
      _foodChainHandler = new FoodChainHandler( this ) ;
      _movementHandler = new PlayerMovementHandler( this ) ;
      _animationHandler = new PlayerAnimationHandler( this ) ;
      _collidingHandler = new PlayerCollidingHandler( this ) ;
      _playerActionHandlers.Add( new KeyboardHandler() ) ;
      _playerActionHandlers.Add( new TouchHandler() ) ;
      // Because touch action on Mobile conflicts with mouse, and I don't know what will happen to Desktop has touch support,
      // So it will be check later if bug occurs
      if ( OS.GetName() != "Android" && OS.GetName() != "iOS" ) {
        _playerActionHandlers.Add( new MouseHandler() ) ;
      }

      /*
      Setup
      */

      _collidingHandler.GetObservableEvent( typeof( KinematicCollision ) ).Subscribe( work =>
      {
        if ( ! ( work.EventArgs.Argument is KinematicCollision collision ) ) return ;
        if ( _foodChainHandler.HandleCollision( collision ) ) {
          _foodPlan.HandleCollision( collision ) ;
        }
      } ) ;
      
      // var gridMap = GetParent().GetNode<GridMap>( "GridMap" ) ;
      // var cellSize = new Vector3( 2, 2, 2 ) ;
      // var cellSizeFloat = 2 ;
      // CollidingCells = new BvhStructure( gridMap?.GetUsedCells().OfType<Vector3>().Select( vector => new BoundingBox( vector * cellSizeFloat, ( vector + cellSize ) * cellSizeFloat ) ).ToList() ) ;
      _randomSpawn = GetParent().GetNode<RandomSpawn>( "RandomSpawn" ) ;
      CallDeferred( nameof( UpdatePlayerPosition ) ) ;
    }

    private void UpdatePlayerPosition()
    {
      _randomSpawn.BoidsPool.UpdatePlayerPosition( this ) ;
      _randomSpawn.Boids2Pool.UpdatePlayerPosition( this ) ;
    }
    // public BoundingBox GetBoundingBox()
    // {
    //   var _avoidDistance = 3f ;
    //   var vector = new Vector3( _avoidDistance, _avoidDistance, _avoidDistance ) ;
    //   return new BoundingBox( this.Translation - vector, this.Translation + vector ) ;
    // }

    // public BvhStructure CollidingCells { get ; set ; }

    public override void _PhysicsProcess( float delta )
    {
      var toDirection = _playerActionHandlers.Aggregate( Vector3.Zero,
        ( current, handler ) => current + handler.GetMovementDirection() ) ;
      var doDash = _playerActionHandlers.Any( handler => PlayerInputHandler.IsActionPressed() ) ;
      var moveDirection = _movementHandler.MoveAndFlip( toDirection, doDash ) ;
      _animationHandler.UpdateAnimation( moveDirection, _movementHandler.IsDashing ) ;
      _collidingHandler.HandleCollider() ;
      // var tilemap = CollidingCells.Overlaps( GetBoundingBox() ) ;
      // if ( tilemap != null ) GD.Print( tilemap.Centroid ) ;
      base._PhysicsProcess( delta ) ;
    }

    public override void _UnhandledInput( InputEvent @event )
    {
      foreach ( var playerActionHandler in _playerActionHandlers ) {
        playerActionHandler.UnhandledInput( @event ) ;
      }

      base._UnhandledInput( @event ) ;
    }

    public bool UpdateFoodPosition( FoodChainPosition position )
    {
      FoodPosition = position ;
      return true ;
    }

    public bool Kill()
    {
      SetPhysicsProcess( false ) ;
      GetTree().ReloadCurrentScene() ;
      return true ;
    }

// #if DEBUG
//     public override void _Process( float delta )
//     {
//       GD.Print( Engine.GetFramesPerSecond() ) ;
//       base._Process( delta ) ;
//     }
// #endif
  }

  public interface IMortalObject
  {
    public bool Kill() ;
  }
}