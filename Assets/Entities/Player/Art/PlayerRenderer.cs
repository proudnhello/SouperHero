using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEditorInternal.VersionControl.ListControl;

public class PlayerRenderer : EntityRenderer
{
    PlayerMovement _playerMovement;
    PlayerAnimationHolder _animations;

    List<State> states;
    State currentState;
    public PlayerRenderer(Entity entity, PlayerAnimationHolder animations) : base(entity)
    {
        _playerMovement = entity.gameObject.GetComponent<PlayerMovement>();
        _animations = animations;

        states = new()
        {
            new MovementState(this, 0),
            new SwingState(this, 1)
        };
        currentState = states[0];
        currentState.Enter();

        PlayerInventory.UsedSpoon += Swing;
        entity.StartCoroutine(Update());
    }

    public void Disable()
    {
        PlayerInventory.UsedSpoon -= Swing;
    }

    public void ChangeState(int index)
    {
        if (currentState.priority <= states[index].priority)
        {
            currentState.Exit();
            currentState = states[index];
            currentState.Enter();
            
        }
    }

    public void StateFinished()
    {
        currentState = states[0];
        currentState.Enter();
    }


    public IEnumerator Update()
    {
        while (Entity.gameObject.activeInHierarchy)
        {
            spriteRenderer.flipX = _playerMovement.currentDirection.x > 0;
            
            yield return null;
        }
    }
    public void Swing()
    {
        ChangeState(1);
    }

    public abstract class State
    {
        public int priority;
        protected PlayerRenderer ren;
        public State(PlayerRenderer ren, int priority) { this.ren = ren; this.priority = priority; }
        protected IEnumerator IUpdate;
        public abstract IEnumerator Update();
        public void Enter()
        {
            ren.Entity.StartCoroutine(IUpdate = Update());
        }
        public virtual void Exit()
        {
            if (IUpdate != null) ren.Entity.StopCoroutine(IUpdate);
        }
    }

    public class MovementState : State
    {
        public MovementState(PlayerRenderer ren, int priority) : base(ren, priority) { }
        public override IEnumerator Update()
        {
            while (true)
            {
                int frame = 0;
                while (ren._playerMovement.currrentMoveSpeed < 0.01)
                {
                    ren.spriteRenderer.sprite = ren._animations.IdleSprites[frame];
                    yield return new WaitForSeconds(1f / ren._animations.IdleFPS);
                    frame = (frame + 1) % ren._animations.IdleSprites.Length;
                }

                frame = 0;
                while (ren._playerMovement.currrentMoveSpeed >= 0.01)
                {
                    ren.spriteRenderer.sprite = ren._animations.WalkSprites[frame];
                    yield return new WaitForSeconds(1f / ren._animations.WalkFPS);
                    frame = (frame + 1) % ren._animations.WalkSprites.Length;
                }

                yield return null;
            }
        }


    }

    public class SwingState : State
    {
        public SwingState(PlayerRenderer ren, int priority) : base(ren, priority) { }

        public override IEnumerator Update()
        {
            for (int frame = 0; frame < ren._animations.AttackSprites.Length; frame++)
            {
                ren.spriteRenderer.sprite = ren._animations.AttackSprites[frame];
                yield return new WaitForSeconds(1f / ren._animations.AttackFPS);
            }
            ren.StateFinished();
        }

        public override void Exit()
        {
            if (IUpdate != null) ren.Entity.StopCoroutine(IUpdate);         
        }
    }
}