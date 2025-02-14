using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideBomberScript : EnemyBaseClass
{
    [SerializeField]
    private float _runSpeed = 5f;
    [SerializeField]
    private float _walkSpeed = 2f;

    public enum BomberState
    {
        IDLING,
        CHASING,
        EXPLODE,
        FATE_SEALED
    }

    private BomberState _state;
    private GameObject _detectionRadius;
    public float _chaseForSeconds = 2.0f;
    private GameObject _explosionPNG;

    protected new void Start()
    {
        base.Start();
        _state = BomberState.IDLING;
        _detectionRadius = transform.GetChild(0).gameObject;
        _explosionPNG = transform.GetChild(1).gameObject;
    }

    protected override void UpdateAI()
    {
        Vector2 direction = _playerTransform.position - transform.position;
        if (_state == BomberState.IDLING)
        {
            direction = direction.normalized;
            direction *= _walkSpeed;
            _rigidbody.velocity = direction;
            if (direction.magnitude < _detectionRadius.transform.localScale.x)
            {
                _state = BomberState.CHASING;
                StartCoroutine("chaseTimer");
            }
        } else if(_state == BomberState.CHASING)
        {
            direction = direction.normalized;
            direction *= _runSpeed;
            _rigidbody.velocity = direction;
        } else if (_state == BomberState.EXPLODE)
        {
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0;

            StartCoroutine("explodeInFire");
            _state = BomberState.FATE_SEALED;
        }
    }

    public IEnumerator chaseTimer()
    {
        yield return new WaitForSeconds(_chaseForSeconds);
        _state = BomberState.EXPLODE;
    }

    public IEnumerator explodeInFire()
    {
        int maxFlashCycles = 5;
        int flashCycles = 0;
        while (maxFlashCycles > flashCycles)
        {
            _sprite.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            _sprite.color = _initialColor;
            yield return new WaitForSeconds(0.15f);
            flashCycles++;
        }

        // do the explosion
        transform.up = Vector3.up;
        _explosionPNG.SetActive(true);

        yield return new WaitForSeconds(1);

        DestroyImmediate(this.gameObject);
    }

    protected new void Update()
    {
        base.Update();
    }
}
