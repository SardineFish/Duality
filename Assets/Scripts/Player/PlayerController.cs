﻿using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using SardineFish.Utils;

namespace Duality
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        const float FIXED_DELTA_TIME = 0.02f;
        [SerializeField] private bool m_EnableControl = true;

        [SerializeField] [Delayed] private float m_Speed = 10;
        [SerializeField] [Range(0, 1)] private float m_MoveDamping = 1f;
        [SerializeField] [Delayed] private float m_JumpHeight = 3;
        [SerializeField] [Delayed] private float m_JumpTime = 0.5f;

        [SerializeField] private float m_FallGravityScale = 1;

        [SerializeField] private float m_JumpCacheTime = 0.2f;

        [SerializeField] private float m_WolfTime = 0.1f;

        [SerializeField]
        private KeyCode ActionKey = KeyCode.J;


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
        StateCache jumpCache = new StateCache(0.2f);
        StateCache onGroundCache = new StateCache(0.1f);

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
            UpdateMovement();
            UpdateAction();
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
                if (!IsColliderTile(mapPos + new Vector2Int(facing, -1)))
                {
                    selectedBlock = mapPos + new Vector2Int(facing, -1);
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
                if (IsColliderTile(mapPos + Vector2Int.right * facing))
                {
                    selectedBlock = mapPos + Vector2Int.right * facing;
                    selected = true;
                }
                else if (IsColliderTile(mapPos + new Vector2Int(facing, -1)))
                {
                    selectedBlock = mapPos + new Vector2Int(facing, -1);
                    selected = true;
                }
            }

            if (selected)
            {
                Utility.DebugDrawRect(new Rect(selectedBlock, Vector2.one), Color.yellow);
            }

            if (selected && Input.GetKeyDown(ActionKey))
            {
                if (holdingTile)
                {
                    holdingTile = GameMap.Instance.SetTileAt(selectedBlock, holdingTile);
                }
                else
                {
                    holdingTile = GameMap.Instance.RemoveTileAt(selectedBlock);
                }
            }
        }

        private void UpdateMovement()
        {
            var movement = new Vector2();
            if (Input.GetKey(KeyCode.A))
                movement += Vector2.left;
            if (Input.GetKey(KeyCode.D))
                movement += Vector2.right;
            if (Input.GetKey(KeyCode.W))
                movement += Vector2.up;
            if (Input.GetKey(KeyCode.S))
                movement += Vector2.down;
            if (Input.GetKey(KeyCode.Space))
                jumpCache.Renew(Time.time);
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space))
            {
                onGroundCache.Renew(Time.time);
                jumpCache.Renew(Time.time);
            }
            Debug.Log(movement);

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
                m_Velocity.y = jumpVelocity;
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

            var g = Vector2.down * gravity;
            if (velocity.y < 0)
                g = Vector2.down * gravity * m_FallGravityScale;

            velocity += g * Time.fixedDeltaTime;

            float distance = 0;
            Vector2 motionStep = Time.fixedDeltaTime * velocity;

            if (CollisionCheck(Time.fixedDeltaTime, velocity, Vector2.down, Vector2.left, out distance))
            {
                motionStep.y = MathUtility.MinAbs(motionStep.y, velocity.normalized.y * distance);
                Land();
            }

            if (CollisionCheck(Time.fixedDeltaTime, velocity, Vector2.up, Vector2.left, out distance))
                motionStep.y = MathUtility.MinAbs(motionStep.y, velocity.normalized.y * distance);
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
            => GameMap.Instance.GetTileAt(pos) as Tile is var tile && tile &&
               tile.colliderType != Tile.ColliderType.None;

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
    }
}