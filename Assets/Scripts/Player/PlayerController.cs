using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using SardineFish.Utils;

namespace Duality
{
    [Serializable]
    struct PlayerInputSettings
    {
        public KeyCode MoveLeft;
        public KeyCode MoveRight;
        public KeyCode Jump;
        public KeyCode Action;
    }

    [Serializable]
    struct PlayerAnimations
    {
        public AnimationClip Idle;
        public AnimationClip JumpUP;
        public AnimationClip JumpFall;
        public AnimationClip Walk;
        public AnimationClip LiftUp;
        public AnimationClip LiftIdle;
        public AnimationClip LiftWalk;
        public AnimationClip PutDown;
    }

    enum PlayerState
    {
        Idle,
        Move,
        JumpUp,
        JumpFall,
        LiftUp,
        PutDown,
    }

    [RequireComponent(typeof(BoxCollider2D), typeof(PlayableTracks))]
    public class PlayerController : AnimationManagerBase
    {
        const float FIXED_DELTA_TIME = 0.02f;
        [SerializeField] private bool m_EnableControl = true;

        [SerializeField] [Delayed] private float m_Speed = 10;
        [SerializeField] [Range(0, 1)] private float m_MoveDamping = 1f;
        [SerializeField] [Delayed] private float m_JumpHeight = 3;
        [SerializeField] [Delayed] private float m_JumpTime = 0.5f;

        [SerializeField]
        private int m_GravityDir = -1;

        [SerializeField] private float m_FallGravityScale = 1;

        [SerializeField] private float m_JumpCacheTime = 0.2f;

        [SerializeField] private float m_WolfTime = 0.1f;

        [SerializeField]
        private PlayerInputSettings InputSettings = new PlayerInputSettings()
        {
            MoveLeft = KeyCode.A,
            MoveRight = KeyCode.D,
            Jump = KeyCode.Space,
            Action = KeyCode.J
        };

        [SerializeField] private List<TileBase> m_ColliderTiles;
        [SerializeField] private List<TileBase> m_MovableTiles;
        [SerializeField] private TileBase m_AirTile;
        [SerializeField] private GameObject PickPrefab;
        [SerializeField] private Transform PickUpHolder;
        [SerializeField] private float PickTimeout;
        [SerializeField] private float PickVelocity = 10;

        [SerializeField] private PlayerAnimations m_Animations;


        float gravity
        {
            get
            {
                var dt = Time.fixedDeltaTime;
                var n = m_JumpTime / dt;
                return 2 * m_JumpHeight / (n * (n + 1) * dt * dt);
            }
        }

        float jumpVelocity => gravity * m_JumpTime;


        private TileBase holdingTile = null;
        bool focused = true;
        Vector2 rawMovementInput;
        Vector2 dampedInput = Vector2.zero;
        Vector2 m_Velocity;
        /// <summary>
        /// 1 -> Right
        /// -1 -> Left
        /// </summary>
        private int facing = 1;
        bool onGround = false;
        new BoxCollider2D collider;
        new SpriteRenderer renderer;
        private PlayableTracks _playableTracks;
        StateCache jumpCache = new StateCache(0.2f);
        StateCache onGroundCache = new StateCache(0.1f);
        private PlayerState State = PlayerState.Idle;
        private GameObject holdingBlockObject;

        public bool EnableControl
        {
            get => m_EnableControl;
            set => m_EnableControl = value;
        }

        public Vector2 velocity
        {
            get => m_Velocity;
            private set => m_Velocity = value;
        }

        private void Awake()
        {
            collider = GetComponent<BoxCollider2D>();
            renderer = GetComponent<SpriteRenderer>();
            if (!renderer)
                renderer = GetComponentInChildren<SpriteRenderer>();
            _playableTracks = GetComponent<PlayableTracks>();

        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        // Use this for initialization
        void Start()
        {
        }

        void Land()
        {
            onGround = true;
            onGroundCache.Renew(Time.time);
        }

        // Update is called once per frame
        void Update()
        {
            switch (State)
            {
                case PlayerState.Idle:
                case PlayerState.Move:
                case PlayerState.JumpUp: 
                case PlayerState.JumpFall:
                    UpdateMovement();
                    UpdateAction();
                    break;
            }
        }

        private async void ChangeState(PlayerState nextState)
        {
            if (State == nextState)
                return;
            State = nextState;
            switch (State)
            {
                case PlayerState.Idle:
                    if (holdingTile)
                        _playableTracks.PlayOnTrack(0, m_Animations.LiftIdle);
                    else
                        _playableTracks.PlayOnTrack(0, m_Animations.Idle);
                    break;
                case PlayerState.Move:
                    if (holdingTile)
                        _playableTracks.PlayOnTrack(0, m_Animations.LiftWalk);
                    else
                        _playableTracks.PlayOnTrack(0, m_Animations.Walk);
                    break;
                case PlayerState.JumpUp:
                    _playableTracks.PlayOnTrack(0, m_Animations.JumpUP);
                    break;
                case PlayerState.JumpFall:
                    _playableTracks.PlayOnTrack(0, m_Animations.JumpFall);
                    break;
                case PlayerState.LiftUp:
                    await _playableTracks.PlayOnTrackWait(0, m_Animations.LiftUp);
                    velocity = Vector2.zero;
                    ChangeState(PlayerState.Idle);
                    break;
                case PlayerState.PutDown:
                    await _playableTracks.PlayOnTrackWait(0, m_Animations.PutDown);
                    velocity=Vector2.zero;
                    
                    ChangeState(PlayerState.Idle);
                    break;
            }
        }

        private void UpdateAction()
        {
            if(!onGround)
                return;
            var mapPos = transform.position.ToVector2Int();
            var selectedBlock = Vector2Int.zero;
            var selected = false;
            if (holdingTile)
            {
                if (!IsColliderTile(mapPos + new Vector2Int(facing, m_GravityDir)))
                {
                    selectedBlock = mapPos + new Vector2Int(facing, m_GravityDir);
                    selected = true;
                }
                else if (!IsColliderTile(mapPos + Vector2Int.right * facing))
                {
                    selectedBlock = mapPos + Vector2Int.right * facing;
                    selected = true;
                }
            }
            else
            {
                if (IsMovableTile(mapPos + Vector2Int.right * facing))
                {
                    selectedBlock = mapPos + Vector2Int.right * facing;
                    selected = true;
                }
                else if (IsMovableTile(mapPos + new Vector2Int(facing, m_GravityDir)))
                {
                    selectedBlock = mapPos + new Vector2Int(facing, m_GravityDir);
                    selected = true;
                }
            }

            if (selected)
            {
                Utility.DebugDrawRect(new Rect(selectedBlock, Vector2.one), Color.yellow);
            }

            if (selected && Input.GetKeyDown(InputSettings.Action))
            {
                if (holdingTile)
                {
                    ChangeState(PlayerState.PutDown);
                    var target = new GameObject();
                    target.transform.position = selectedBlock.ToVector3(GameMap.Instance.transform.position.z - 1);
                    target.transform.Translate(new Vector3(0.5f, 0.5f, 0));
                    holdingBlockObject.GetComponent<CriticalDamping>().Target = target.transform;
                    this.SetTimeout(() =>
                    {
                        holdingTile = GameMap.Instance.SetTileAt(selectedBlock, holdingTile);
                        ChangeState(PlayerState.Idle);
                        holdingTile = null;
                        Destroy(holdingBlockObject);
                        Destroy(target);
                    }, PickTimeout);
                }
                else
                {
                    holdingTile = GameMap.Instance.SetTileAt(selectedBlock, m_AirTile);
                    ChangeState(PlayerState.LiftUp);
                    var obj = Instantiate(PickPrefab);
                    obj.transform.position = selectedBlock.ToVector3(GameMap.Instance.transform.position.z - 1);
                    obj.transform.Translate(new Vector3(0.5f, 0.5f, 0));
                    var dampping = obj.GetOrAddComponent<CriticalDamping>();
                    dampping.velocity = Vector3.up * PickVelocity;
                    dampping.Target = PickUpHolder;
                    holdingBlockObject = obj;
                    this.SetTimeout(() =>
                    {
                        dampping.Target = null;
                        obj.transform.SetParent(PickUpHolder);
                    }, PickTimeout);
                }
            }
        }

        private void UpdateMovement()
        {
            var movement = new Vector2();
            if (Input.GetKey(InputSettings.MoveLeft))
                movement += Vector2.left;
            if (Input.GetKey(InputSettings.MoveRight))
                movement += Vector2.right;
            if (Input.GetKey(InputSettings.Jump))
                jumpCache.Renew(Time.time);
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(InputSettings.Jump))
            {
                onGroundCache.Renew(Time.time);
                jumpCache.Renew(Time.time);
            }

            rawMovementInput = movement;
            jumpCache.CacheTime = m_JumpCacheTime;
            jumpCache.Update(Time.time);
            onGroundCache.CacheTime = m_WolfTime;
            onGroundCache.Update(Time.time);

            if (rawMovementInput.x < 0)
            {
                renderer.flipX = true;
                facing = -1;
            }
            else if (rawMovementInput.x > 0)
            {
                renderer.flipX = false;
                facing = 1;
            }

            if (State == PlayerState.JumpUp || State == PlayerState.JumpFall)
            {
                if (onGround)
                    ChangeState(PlayerState.Idle);
            }
            
            if (State == PlayerState.Idle && rawMovementInput.x != 0)
                ChangeState(PlayerState.Move);
            if (State == PlayerState.Move && rawMovementInput.x == 0)
                ChangeState(PlayerState.Idle);
            
        }
        

        public void SetPositionVelocity(Vector2 pos, Vector2 velocity)
        {
            transform.position = pos.ToVector3(transform.position.z);
            this.velocity = velocity;
        }

        public void Jump()
        {
            if (onGroundCache)
            {
                m_Velocity.y = jumpVelocity * -m_GravityDir;
                ChangeState(PlayerState.JumpUp);
            }

            onGround = false;
            onGroundCache.Clear();
        }

        private void FixedUpdate()
        {
            if (!EnableControl)
                return;

            if (jumpCache.Value)
                Jump();

            onGround = false;

            velocity = new Vector2(
                Mathf.Lerp(velocity.x, rawMovementInput.x * m_Speed, (1 - m_MoveDamping)),
                velocity.y
            );

            var g = Vector2.up * m_GravityDir * gravity;
            if (MathUtility.SignInt(velocity.y)  == m_GravityDir)
                g = Vector2.up * gravity * m_GravityDir * m_FallGravityScale;

            velocity += g * Time.fixedDeltaTime;

            float distance = 0;
            Vector2 motionStep = Time.fixedDeltaTime * velocity;

            if (CollisionCheck(Time.fixedDeltaTime, velocity, Vector2.down, Vector2.left, out distance))
            {
                motionStep.y = MathUtility.MinAbs(motionStep.y, velocity.normalized.y * distance);
                if (m_GravityDir < 0)
                    Land();
            }

            if (CollisionCheck(Time.fixedDeltaTime, velocity, Vector2.up, Vector2.left, out distance))
            {
                motionStep.y = MathUtility.MinAbs(motionStep.y, velocity.normalized.y * distance);
                if (m_GravityDir > 0)
                    Land();
            }
            if (CollisionCheck(Time.fixedDeltaTime, velocity, Vector2.left, Vector2.up, out distance))
                motionStep.x = MathUtility.MinAbs(motionStep.x, velocity.normalized.x * distance);
            if (CollisionCheck(Time.fixedDeltaTime, velocity, Vector2.right, Vector2.up, out distance))
                motionStep.x = MathUtility.MinAbs(motionStep.x, velocity.normalized.x * distance);

            velocity = motionStep / Time.fixedDeltaTime;
            Debug.Log(motionStep);

            transform.position += motionStep.ToVector3();
        }

        Rect CreateRect(Vector2 center, Vector2 size) => new Rect(center - (size / 2), size);


        bool IsColliderTile(Vector2 pos)
            => GameMap.Instance.GetTileAt(pos) as TileBase is var tile && tile &&
               m_ColliderTiles.Contains(tile);
        
        bool IsMovableTile(Vector2 pos)
            => GameMap.Instance.GetTileAt(pos) as TileBase is var tile && tile &&
               m_MovableTiles.Contains(tile);

        List<Rect> neighboorTiles = new List<Rect>();

        bool CollisionCheck(float dt, Vector2 velocity, Vector2 normal, Vector2 tangent, out float hitDistance)
        {
            var colliderSize = collider.size + Vector2.one * collider.edgeRadius * 2;

            // Trim collider width a little bit to let player fall down when jumping clinging to a wall
            if (normal.y != 0)
                colliderSize.x -= 0.001f;

            var halfSize = colliderSize / 2;
            var pointA = collider.transform.position.ToVector2() + collider.offset + halfSize * normal +
                         halfSize * tangent;
            var pointB = collider.transform.position.ToVector2() + collider.offset + halfSize * normal -
                         halfSize * tangent;

            var center = collider.transform.position.ToVector2() + collider.offset;
            var offset = velocity * dt;
            neighboorTiles.Clear();

            if (!IsColliderTile(center + tangent) && IsColliderTile(center + normal + tangent))
            {
                neighboorTiles.Add(new Rect(MathUtility.Floor(center + normal + tangent) - halfSize,
                    Vector2.one + colliderSize));
            }

            if (!IsColliderTile(center - tangent) && IsColliderTile(center + normal - tangent))
            {
                neighboorTiles.Add(new Rect(MathUtility.Floor(center + normal - tangent) - halfSize,
                    Vector2.one + colliderSize));
            }

            if (IsColliderTile(center + normal))
            {
                neighboorTiles.Add(new Rect(MathUtility.Floor(center + normal) - halfSize, Vector2.one + colliderSize));
            }

            var minDistance = float.MaxValue;
            bool colliderHit = false;
            foreach (var tile in neighboorTiles)
            {
                Utility.DebugDrawRect(tile, new Color(normal.x * .5f + .5f, normal.y * .5f + .5f, 1),
                    Mathf.Atan2(normal.y, normal.x));

                float THRESHOLD = -0.00001f;

                var (hit, distance, norm) = MathUtility.BoxRaycast(tile, center, velocity.normalized);
                if (hit && THRESHOLD <= distance && distance <= offset.magnitude && Vector2.Dot(norm, normal) < -0.99f)
                {
                    Debug.DrawLine(center + velocity.normalized * distance,
                        center + velocity.normalized * distance + norm);
                    minDistance = Mathf.Min(distance, minDistance);
                    colliderHit = true;
                }
            }

            hitDistance = minDistance;

            return colliderHit;
        }

        Vector2 ClampToDir(Vector2 v, Vector2 normal)
        {
            if (normal.x > 0)
                v.x = Mathf.Ceil(v.x);
            else if (normal.x < 0)
                v.x = Mathf.Floor(v.x);

            if (normal.y > 0)
                v.y = Mathf.Ceil(v.y);
            else if (normal.y < 0)
                v.y = Mathf.Floor(v.y);

            return v;
        }

        private void OnApplicationFocus(bool focus)
        {
            focused = focus;
        }

        public override void GetAnimationClips(List<AnimationClip> results)
        {
            base.GetAnimationClips(results);
            
            GetAnimationClipMember(m_Animations, results);
        }
    }
}