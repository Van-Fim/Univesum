using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;

public class ShipController : SpaceObjectController
{
    public override void Turn(Vector3 position)
    {
        base.Turn(position);
        if (Sp_object == null) return;
        Ship ship = (Ship)Sp_object;
        if (ship.engine == null) return;
        int _rotationSpeed = ship.engine.rotationSpeed;
        // 1. Получаем направление к цели
        Vector3 directionToTarget = position - transform.position;

        // 2. Игнорируем расстояние по вертикали (опционально, для космоса)
        // directionToTarget.y = 0; // Раскомментировать для 2D-подобного поворота

        // 3. Создаем целевой поворот
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // 4. Плавно поворачиваемся с ограничением максимальной скорости
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime
        );
    }
    public override void TurnDir(Vector3 direction)
    {
        base.TurnDir(direction);
        if (Sp_object == null) return;
        Ship ship = (Ship)Sp_object;
        if (ship.engine == null) return;
        int _rotationSpeed = ship.engine.rotationSpeed;
    }
    #region Movement Logic
    public override void Move(Vector3 targetPosition)
    {
        if (Sp_object == null) return;
        Ship ship = (Ship)Sp_object;
        if (ship.engine == null) return;

        Rigidbody rb = ship.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Рассчитываем направление к цели
        Vector3 direction = (targetPosition - transform.position).normalized;
        float speed = ship.engine.maxSpeed;

        // Вариант 1: Прямое управление скоростью (для аркадного управления)
        rb.linearVelocity = direction * speed;

        // Вариант 2: Прикладывание силы (для реалистичной инерции)
        // rb.AddForce(direction * ship.engine.accelerationForce);
    }

    public override void Move(float spfc = -1f, Transform target = null)
    {
        if (Sp_object.is_destroyed || _rigidbody == null)
            return;
        Ship ship = (Ship)Sp_object;
        if (ship.engine == null)
        {
            return;
        }
        int _accelerationSpeed = ship.engine.accelerationSpeed;
        int _maxSpeed = ship.engine.maxSpeed;
        if (Input.GetKey(KeyCode.Space) && _rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _currentSpeedFactor = _targetSpeedFactor = 0f;
            return;
        }

        float speedChangeFactor = spfc;
        _targetSpeedFactor += speedChangeFactor;

        // Ограничение целевого фактора скорости
        _targetSpeedFactor = Mathf.Clamp(
            _targetSpeedFactor,
            MinSpeedFactor,
            MaxSpeedFactor
        );

        // Плавное изменение текущего фактора скорости
        if (_currentSpeedFactor < _targetSpeedFactor)
        {
            _currentSpeedFactor += _accelerationSpeed * Time.fixedDeltaTime;
            if (_currentSpeedFactor > _targetSpeedFactor)
            {
                _currentSpeedFactor = _targetSpeedFactor;
            }
        }
        else if (_currentSpeedFactor > _targetSpeedFactor)
        {
            _currentSpeedFactor -= _accelerationSpeed * Time.fixedDeltaTime;
            if (_currentSpeedFactor < _targetSpeedFactor)
            {
                _currentSpeedFactor = _targetSpeedFactor;
            }
        }

        // Применение силы движения
        _rigidbody.linearVelocity = (transform.forward * _maxSpeed * _currentSpeedFactor);
    }
    #endregion
}
