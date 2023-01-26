using System.Collections.Generic ;
using System.Linq ;
using System.Threading.Tasks ;
using Fish.Scripts.Utilities ;
using Fish.Scripts.Utilities.FoodChain ;
using Godot ;

namespace Fish.Scripts.Nodes
{
  public class Boid : KinematicBody, ICell, IBoundingBoxGetter, IFood, IMortalObject
  {
    [Export]
    protected float MaxSpeed = 16 ;

    [Export]
    protected float MinSpeed = 10 ;

    [Export]
    protected float TargetForce = 0.3f ;

    [Export]
    protected float CohesionForce = 0.9f ;

    [Export]
    protected float AlignmentForce = 0.4f ;

    [Export]
    protected float SeparationForce = 0.3f ;

    [Export]
    protected float AvoidForce = 3.9f ;

    [Export]
    public float ViewDistance = 5f ;

    [Export]
    protected float AvoidDistance = 5f ;

    [Export]
    protected int MaxFlockSize = 15 ;

    protected KinematicBody _player ;
    private const string GraphicsPath = "Graphics" ;
    private const string AnimationPlayerPath = "Graphics/AnimationPlayer" ;
    private const string AnimationSwimmingFastName = "Swimming_Fast" ;
    private const string VisibilityNotifierPath = "VisibilityNotifier" ;
    private const string ScreenEnteredEventName = "screen_entered" ;
    private const string ScreenExitedEventName = "screen_exited" ;
    private readonly bool _doWrapScreen = true ;
    private Spatial _graphics ;
    private VisibilityNotifier _visibilityNotifier ;
    private AnimationPlayer _animationPlayer ;
    private Vector2 _screenSize ;
    private Vector2 _wrapScreenSize ;
    private Vector3 _velocity ;
    private readonly List<Vector3> _targets = new List<Vector3>() ;
    protected float RaiseDegreesX ;
    protected float RaiseDegreesZ ;
    private float _playerAvoidDistance ;
    public ICollection<Boid> Flock { get ; set ; }
    public bool IsDisabled { get ; private set ; }
    public BvhStructure CollidingCells { get ; set ; }
    public virtual FoodChainPosition FoodPosition { get ; protected set ; } = FoodChainPosition.RedSnapper ;

    public override void _Ready()
    {
      // _screenSize = GetViewport().Size ;
      _screenSize = GraphicsExtensions.GameWorldScreenSize ;
      var wrapPaddingDistance = 20f ;
      _wrapScreenSize = _screenSize + new Vector2( wrapPaddingDistance, wrapPaddingDistance ) ;
      _graphics = GetNode<Spatial>( GraphicsPath ) ;
      _animationPlayer = GetNode<AnimationPlayer>( AnimationPlayerPath ) ;
      _animationPlayer.Play( AnimationSwimmingFastName ) ;
      _animationPlayer.GetAnimation( AnimationSwimmingFastName ).Loop = true ;
      GD.Randomize() ;
      _animationPlayer.PlaybackSpeed = (float)GD.RandRange( 2.5d, 3d ) ;
      // _visibilityNotifier = GetNode<VisibilityNotifier>( VisibilityNotifierPath ) ;
      // if ( _visibilityNotifier != null ) {
      //   _visibilityNotifier.Connect( ScreenEnteredEventName, this, nameof( ShowBoid ) ) ;
      //   _visibilityNotifier.Connect( ScreenExitedEventName, this, nameof( HideBoid ) ) ;
      //   Visible = false ;
      // }

      GD.Randomize() ;
      RaiseDegreesX = (float)GD.RandRange( 10, 30 ) ;
      GD.Randomize() ;
      RaiseDegreesZ = (float)GD.RandRange( 5, 30 ) ;
      _velocity = new Vector3( (float)GD.RandRange( -1d, 1d ), (float)GD.RandRange( -1d, 1d ), 0 ) * MinSpeed ;
      GD.Randomize() ;
      _playerAvoidDistance = (float)( AvoidDistance * ( GD.Randf() + 0.3 ) ) ;
      base._Ready() ;
    }

    private void ShowBoid()
    {
      Visible = true ;
    }

    private void HideBoid()
    {
      Visible = false ;
    }

    public override void _PhysicsProcess( float delta )
    {
      if ( IsDisabled ) return ;
      _velocity = CalculateLinearVelocity( _velocity ) ;
      MoveAndSlide( _velocity, Vector3.Up ) ;
      base._PhysicsProcess( delta ) ;
    }

    private Vector3 CalculateLinearVelocity( Vector3 velocity )
    {
      if ( _doWrapScreen ) WrapScreen() ;
      var avoidVector = AvoidObstacles() ;
      var (cohesionVector, alignVector, separationVector) = GetFlockStatus() ;
      cohesionVector *= CohesionForce ;
      alignVector *= AlignmentForce ;
      separationVector *= SeparationForce ;
      var additionalVelocity = cohesionVector + alignVector + separationVector ;
      if ( _targets.Count > 0 ) {
        var targetVector =
          _targets.Aggregate( Vector3.Zero, ( current, target ) => current + GlobalTranslation.DirectionTo( target ) ) /
          _targets.Count ;
        additionalVelocity += targetVector * TargetForce ;
      }

      // Keep fish swims in 2D by converging Translation.Z to 0
      velocity.z = -Translation.z ;
      velocity = velocity.LimitLength( MaxSpeed ) ;
      if ( velocity.Length() < MinSpeed ) ( velocity * MinSpeed ).LimitLength( MaxSpeed ) ;
      velocity += avoidVector * AvoidForce ;
      velocity = velocity.LinearInterpolate( velocity + additionalVelocity, 0.2f ) ;
      velocity.Flip( _graphics, RaiseDegreesX, RaiseDegreesZ ) ;
      return velocity ;
    }

    private Vector3 AvoidObstacles()
    {
      var edgeAvoidVector = Vector3.Zero ;

      // Avoid screen edge
      if ( ! _doWrapScreen ) {
        if ( Translation.x - AvoidDistance < 0 ) edgeAvoidVector.x = 1 ;
        else if ( Translation.x + AvoidDistance > _screenSize.x ) edgeAvoidVector.x = -1 ;
        if ( Translation.y - AvoidDistance < 0 ) edgeAvoidVector.y = 1 ;
        else if ( Translation.y + AvoidDistance > _screenSize.y ) edgeAvoidVector.y = -1 ;
        if ( edgeAvoidVector != Vector3.Zero ) return edgeAvoidVector ;
      }

      // Avoid tilemaps
      var collideTileMap = CollidingCells.Overlaps( GetBoundingBox() ) ;
      if ( collideTileMap is { } ) {
        if ( _velocity.x < 0 ) edgeAvoidVector.x = -0.3f ;
        else if ( _velocity.x > 0 ) edgeAvoidVector.x = 0.3f ;
        if ( Translation.y < collideTileMap.Centroid.y ) edgeAvoidVector.y = -1 ;
        else if ( Translation.y > collideTileMap.Centroid.y ) edgeAvoidVector.y = 1 ;
      }

      if ( edgeAvoidVector != Vector3.Zero ) return edgeAvoidVector ;
      // edgeAvoidVector.y -= collideTileMap.Centroid.y - Translation.y ;
      // Use approximate calculation tan = opposite / adjacent
      // var newDirection = collideTileMap.Centroid - Translation ;
      // edgeAvoidVector = newDirection.Rotated( Vector3.Forward, Mathf.Atan( ( collideTileMap.Max - collideTileMap.Centroid ).Length() / newDirection.Length() ) ) ;

      // Avoid player
      edgeAvoidVector = UpdateVelocityToPlayer() ;
      return edgeAvoidVector ;
    }

    protected virtual Vector3 UpdateVelocityToPlayer()
    {
      var edgeAvoidVector = Vector3.Zero ;

      if ( _player is { } ) {
        if ( Translation.DistanceTo( _player.Translation ) > _playerAvoidDistance ) return edgeAvoidVector ;
        if ( Translation.x < _player.Translation.x ) edgeAvoidVector.x = -1f ;
        else if ( Translation.x > _player.Translation.x ) edgeAvoidVector.x = 1f ;
        if ( Translation.y < _player.Translation.y ) edgeAvoidVector.y = -1f ;
        else if ( Translation.y > _player.Translation.y ) edgeAvoidVector.y = 1f ;
      }

      return edgeAvoidVector ;
    }

    private void WrapScreen()
    {
      var newTranslation = new Vector3( Mathf.Wrap( Translation.x, 0, _wrapScreenSize.x ),
        Mathf.Wrap( Translation.y, 0, _wrapScreenSize.y ), 0 ) ;

      var doReset = newTranslation != Translation ;
      Translation = newTranslation ;
      if ( doReset ) ResetPhysicsInterpolation() ;
    }

    private (Vector3 center, Vector3 align, Vector3 avoid) GetFlockStatus()
    {
      var (centerVector, flockCenter, alignVector, avoidVector, otherCount) =
        ( Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, 0 ) ;
      if ( Flock is null ) return ( centerVector, alignVector, avoidVector ) ;

      foreach ( var node in Flock ) {
        if ( otherCount == MaxFlockSize ) break ;
        if ( node == this ) continue ;
        if ( node == null || node.IsInsideTree() == false ) {
          continue ;
        }

        var otherPosition = node.GlobalTranslation ;
        var otherVelocity = node._velocity ;
        if ( ! ( GlobalTranslation.DistanceTo( otherPosition ) is var distance ) ||
             ! ( distance < ViewDistance ) ) continue ;
        otherCount += 1 ;
        alignVector += otherVelocity ;
        flockCenter += otherPosition ;
        if ( distance < AvoidDistance ) avoidVector -= otherPosition - GlobalTranslation ;
      }

      if ( otherCount > 0 ) {
        alignVector /= otherCount ;
        flockCenter /= otherCount ;
        centerVector = GlobalTranslation.DirectionTo( flockCenter ) ;
      }

      return ( centerVector, alignVector, avoidVector ) ;
    }

    public void AddTarget( Vector3 targetPosition )
    {
      _targets.Add( targetPosition ) ;
    }

    public void ClearTarget()
    {
      _targets.Clear() ;
    }

    private async Task ReturnToPool()
    {
      HideBoid() ;
      IsDisabled = true ;
      SetPhysicsProcess( false ) ;
      SetProcess( false ) ;
      var parent = GetParent() ;
      parent?.RemoveChild( this ) ;
      await Task.Delay( 1000 ) ;
      var spawnOnRightScreen = GD.Randf() > 0.5f ;
      this.Translation = new Vector3( ( spawnOnRightScreen ? 1 : -1 ) * _wrapScreenSize.x,
        GD.Randf() * _wrapScreenSize.y, 0 ) ;
      parent?.AddChild( this ) ;
      ShowBoid() ;
      IsDisabled = false ;
      SetPhysicsProcess( true ) ;
      SetProcess( true ) ;

      // Translation = ( GetViewport().Size * -1 ).ToVector3( 0 ) ;
      // ResetPhysicsInterpolation() ;
    }

    public void UpdatePlayerPosition( KinematicBody translation )
    {
      _player = translation ;
    }

    public Vector2 ScaledPoint { get ; set ; }

    public BoundingBox GetBoundingBox()
    {
      var vector = new Vector3( AvoidDistance, AvoidDistance, AvoidDistance ) ;
      return new BoundingBox( Translation - vector, Translation + vector ) ;
    }

    private async Task<bool> KillImplement()
    {
      await ReturnToPool() ;
      return true ;
    }

    public bool Kill()
    {
      _ = KillImplement() ;
      return true ;
    }
  }
}