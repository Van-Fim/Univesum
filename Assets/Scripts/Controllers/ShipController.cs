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
    public override void Move(Vector3 position)
    {
        base.Move(position);
        if (Sp_object == null) return;
        Ship ship = (Ship)Sp_object;
        // Debug.Log($"{Sp_object.spaceObjectController.parameters.Count}   {target}   {Sp_object.galaxyId}:{Sp_object.systemId}");
        if (ship.engine == null) return;
        int speed = ship.engine.maxSpeed;
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}