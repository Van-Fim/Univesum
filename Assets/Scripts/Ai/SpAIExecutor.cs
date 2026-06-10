using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class SpAIExecutor : MonoBehaviour
{
    private Ship ship;
    private SpaceObjectController controller;
    void Start()
    {
        ship = GetComponent<Ship>();
        controller = ship.spaceObjectController;
        // Инициализация команды "Idle"
        if (controller == null)
            controller.SetCommand("Idle");
    }

    void Update()
    {

    }

    public void ExecuteCommand()
    {
        if (controller == null)
        {
            return;
        }
        switch (controller.mainCommand)
        {
            case "Idle":
                // Просто дрейфуем или стоим на месте

                break;

            case "FlyTo":
                ExecuteFlyTo();
                break;

            case "Attack":
                ExecuteAttack();
                break;

            case "Follow":
                ExecuteFollow();
                break;

            case "Dock":
                ExecuteDock();
                break;
        }
    }

    void ExecuteFlyTo()
    {
        // Параметры: controller.parameters[0] = X, [1] = Y, [2] = Z
        if (controller.parameters.Count >= 3)
        {
            Vector3 targetPos = new Vector3(
                float.Parse(controller.parameters[0]),
                float.Parse(controller.parameters[1]),
                float.Parse(controller.parameters[2])
            );

            // Движение к точке
            MoveTowards(targetPos);

            // Проверка достижения цели
            if (Vector3.Distance(transform.position, targetPos) < 50f)
            {
                // Команда выполнена - переходим в Idle
                controller.SetCommand("Idle");
                Debug.Log($"{ship.name}: Достиг цели!");
            }
        }
    }

    void ExecuteAttack()
    {
        // Параметр: controller.parameters[0] - GameObject ID или имя цели
        if (controller.parameters.Count > 0)
        {
            // Находим цель (в реальном коде нужен менеджер объектов)
            Ship target = FindTargetByName(controller.parameters[0]);

            if (target != null)
            {
                // Летим к цели
                MoveTowards(target.transform.position);

                // Стреляем, если в радиусе атаки
                if (Vector3.Distance(transform.position, target.transform.position) < 300f)
                {
                    // ShootAt(target);
                }
            }
            else
            {
                // Цель уничтожена - ищем новую или возвращаемся в Idle
                Debug.Log($"{ship.name}: Цель уничтожена!");
                controller.SetCommand("Idle");
            }
        }
    }

    void ExecuteFollow()
    {
        SpaceObject target = null;
        Vector3 targetPos = Vector3.zero;
        
        if (controller.parameters.Count > 1)
        {
            if (controller.parameters[0] == "player")
            {
                target = PlayerService.singleton._player_sp_object;
                targetPos = target.transform.position;
            }
            
            if (target)
            {
                float dst = Vector3.Distance(transform.position, targetPos);
                controller.Turn(target.transform);
                
                if (dst > int.Parse(controller.parameters[1]))
                {
                    controller.Move(target.transform);
                }
                else
                {
                    Debug.Log($"{dst}   {int.Parse(controller.parameters[1])}   {target.galaxyId}:{target.systemId}");
                }
            }
        }
        // Параметр: controller.parameters[0] - объект за которым следовать
        if (controller.parameters.Count > 0)
        {
            // Ship leader = FindTargetByName(controller.parameters[0]);
            // if (leader != null)
            // {
            //     // Держимся позади лидера на дистанции 200 метров
            //     Vector3 followPos = leader.transform.position - leader.transform.forward * 200f;
            //     MoveTowards(followPos);
            // }
        }
    }

    void ExecuteDock()
    {
        // Стыковка со станцией
        // В X3 это сложный процесс с запросом разрешения и т.д.
        // Упрощенная версия:
        Station station = FindStationByName(controller.parameters[0]);
        if (station != null)
        {
            // MoveTowards(station.dockingPort.position);
            // if (Vector3.Distance(transform.position, station.dockingPort.position) < 20f)
            // {
            //     Debug.Log($"{ship.objectName}: Пристыкован к {station.stationName}");
            //     ship.isDocked = true;
            // }
        }
    }

    void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * ship.engine.maxSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    Ship FindTargetByName(string name)
    {
        // Реализуйте поиск объектов на сцене
        GameObject obj = GameObject.Find(name);
        if (obj != null) return obj.GetComponent<Ship>();
        return null;
    }

    Station FindStationByName(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) return obj.GetComponent<Station>();
        return null;
    }
}