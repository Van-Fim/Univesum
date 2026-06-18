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
    public List<Waypoint> waypoints = new List<Waypoint>();
    private int currentWaypointIndex = -1;
    public Waypoint currentWaypoint;
    private float waypointGenerationCooldown = 0f;
    private const float MIN_WAYPOINT_DISTANCE = 2000f; // Минимальная дистанция между waypoint'ами
    void Start()
    {
        ship = GetComponent<Ship>();
        controller = ship.spaceObjectController;
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

            case "Patrol":
                ExecutePatrol();
                break;

            case "Attack":
                ExecuteAttack();
                break;

            case "Follow":

                break;

            case "Dock":
                ExecuteDock();
                break;
        }
    }

    public bool CheckEnemiesInRange(float range)
    {
        bool result = false;
        for (int i = 0; i < ship._StarSystem.ships.Count; i++)
        {
            Ship fs = ship._StarSystem.ships[i];
            if (fs == null) continue;
            if (fs == ship) continue;

            Faction faction = ship.owner;
            FactionRelationshipConfig relConf = faction.factionConfig.relationships[fs.owner.id];
            int rel = 0;
            if (relConf != null) rel = relConf.relation;
            Vector3 targetPos = fs.transform.position;
            if (fs.is_player)
            {
                targetPos = fs.transform.position;
            }
            float distance = Vector3.Distance(ship.transform.position, targetPos);
            if (fs.GetStarSystem() == ship.GetStarSystem() && distance < range)
            {
                if (faction != null && rel < -10000)
                {
                    Debug.Log($"Enemy found: {fs.name}");
                    result = true;
                    break;
                }
            }
        }
        return result;
    }
    void ExecutePatrol()
    {
        if (controller.command == null)
        {
            controller.SetCommand("Idle");
        }
        switch (controller.command)
        {
            case "Idle":
                controller.parameters = controller.mainParameters;
                PatrolIdle();
                break;
            case "FollowAttack":
                FollowAttack();
                break;
            case "Attack":
                SpaceObject target = null;
                Vector3 targetPos = Vector3.zero;

                if (controller.parameters[0] == "player")
                {
                    target = PlayerService.singleton._player_sp_object;
                    targetPos = target.transform.position;
                }
                float dst = Vector3.Distance(transform.position, targetPos);
                controller.Turn(targetPos);
                Debug.Log($"Distance to target: {dst} {controller.parameters[1]}");
                if (dst > int.Parse(controller.parameters[1]))
                {
                    List<string> parameters = "player;500;10000".Split(';').ToList();
                    controller.SetCommand("FollowAttack", parameters);
                }
                else
                {
                    Debug.Log($"Pew! Pew!");
                }

                break;
        }
    }
    void PatrolIdle()
    {
        // Параметры: [0] maxJumps, [1] maxRange, [2] maxHeight, [3] waypointsCount, [4] successWaypointDistance
        if (controller.parameters.Count < 5)
        {
            Debug.LogError($"Patrol command missing parameters! Need 5, got {controller.mainParameters.Count}");
            return;
        }

        int maxJumps = int.Parse(controller.parameters[0]);
        int maxRange = int.Parse(controller.parameters[1]);
        int maxHeight = int.Parse(controller.parameters[2]);
        int waypointsCount = int.Parse(controller.parameters[3]);
        float successWaypointDistance = float.Parse(controller.parameters[4]);
        if (CheckEnemiesInRange(10000))
        {
            List<string> parameters = "player;500;10000".Split(';').ToList();
            controller.SetCommand("FollowAttack", parameters);
        }

        // 1. Генерация waypoint'ов если их нет
        if (waypoints.Count == 0)
        {
            GenerateWaypoints(waypointsCount, maxRange, maxHeight);
        }

        // 2. Выбор или обновление текущего waypoint'а
        if (currentWaypoint == null && waypoints.Count > 0)
        {
            SelectNextWaypoint();
        }

        // 3. Если нет waypoint'ов - выходим
        if (currentWaypoint == null || waypoints.Count == 0)
        {
            // Патруль завершен - переходим в Idle

            Debug.Log($"{ship.id}: Патруль завершен, нет активных точек");
            return;
        }
        Vector3 wpPosition = currentWaypoint.position;

        // Отладка (только если игрок в той же системе)
        if (PlayerService.singleton.GetStarSystem() == ship._StarSystem)
        {
            wpPosition = SpaceContainer.singleton.transform.position + currentWaypoint.position;
            // Debug.Log($"{ship.id}: Waypoints left: {waypoints.Count}, Current WP: {currentWaypointIndex}, Distance: {distanceToWaypoint:F0}");
        }

        // 4. Логика движения к текущему waypoint'у
        float distanceToWaypoint = Vector3.Distance(transform.position, wpPosition);

        // Поворачиваемся к waypoint'у
        controller.Turn(wpPosition);

        // Двигаемся или уничтожаем waypoint при достижении
        if (distanceToWaypoint > successWaypointDistance)
        {
            controller.Move(wpPosition);
        }
        else
        {
            // Достигли waypoint'а - удаляем его
            // Debug.Log($"{ship.id}: Достиг waypoint {currentWaypointIndex} на дистанции {distanceToWaypoint:F0}");
            currentWaypoint.Destroy();
            currentWaypoint = null;

            // Небольшая задержка перед выбором следующего (через корутину или в следующем кадре)
            // Это предотвратит мгновенное переключение
        }
    }
    // Добавьте метод для генерации waypoint'ов
    void GenerateWaypoints(int count, int maxRange, int maxHeight)
    {
        waypoints.Clear();

        for (int i = 0; i < count; i++)
        {
            Vector3 newPosition = GenerateValidWaypointPosition(maxRange, maxHeight);

            // Проверяем, не слишком ли близко к существующим waypoint'ам
            bool tooClose = false;
            foreach (var existingWP in waypoints)
            {
                if (Vector3.Distance(existingWP.position, newPosition) < MIN_WAYPOINT_DISTANCE)
                {
                    tooClose = true;
                    break;
                }
            }

            // Если слишком близко - генерируем заново (с ограничением попыток)
            if (tooClose && i > 0)
            {
                int attempts = 0;
                while (tooClose && attempts < 10)
                {
                    newPosition = GenerateValidWaypointPosition(maxRange, maxHeight);
                    tooClose = false;

                    foreach (var existingWP in waypoints)
                    {
                        if (Vector3.Distance(existingWP.position, newPosition) < MIN_WAYPOINT_DISTANCE)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    attempts++;
                }
            }

            Waypoint waypoint = new Waypoint(newPosition, waypoints.Count());
            waypoint.spAIExecutor = this;
            waypoints.Add(waypoint);
        }

        // Debug.Log($"{ship.id}: Сгенерировано {waypoints.Count} waypoint'ов в радиусе {maxRange}");
    }

    // Вспомогательный метод для генерации позиции waypoint'а
    Vector3 GenerateValidWaypointPosition(int maxRange, int maxHeight)
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * maxRange;
        int randomY = UnityEngine.Random.Range(-maxHeight / 2, maxHeight / 2);

        return new Vector3(randomCircle.x, randomY, randomCircle.y);
    }

    // Выбор следующего waypoint'а (можно случайный или последовательный)
    void SelectNextWaypoint()
    {
        if (waypoints.Count == 0) return;

        // Случайный выбор (как в вашем коде)
        int randomIndex = UnityEngine.Random.Range(0, waypoints.Count);
        currentWaypoint = waypoints[randomIndex];
        currentWaypointIndex = randomIndex;

        // Debug.Log($"{ship.id}: Выбран waypoint {waypoints[randomIndex]} {currentWaypointIndex} из {waypoints.Count}");
    }

    // Альтернативный метод - последовательный выбор (более предсказуемый)
    void SelectNextWaypointSequential()
    {
        if (waypoints.Count == 0) return;

        if (currentWaypointIndex >= waypoints.Count - 1)
        {
            currentWaypointIndex = 0; // Зацикливаем
        }
        else if (currentWaypointIndex < 0)
        {
            currentWaypointIndex = 0;
        }
        else
        {
            currentWaypointIndex++;
        }

        currentWaypoint = waypoints[currentWaypointIndex];
    }
    void ExecuteFlyTo()
    {
        // Параметры: controller.parameters[0] = X, [1] = Y, [2] = Z
        if (controller.mainParameters.Count >= 4)
        {
            Vector3 targetPos = new Vector3(
                float.Parse(controller.mainParameters[0]),
                float.Parse(controller.mainParameters[1]),
                float.Parse(controller.mainParameters[2])
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
        if (controller.mainParameters.Count > 0)
        {
            // Находим цель (в реальном коде нужен менеджер объектов)
            Ship target = FindTargetByName(controller.mainParameters[0]);

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

    void FollowAttack()
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
                controller.Turn(targetPos);
                Debug.Log($"Distance to target: {dst} {controller.parameters[1]}");
                if (dst > int.Parse(controller.parameters[1]))
                {
                    controller.Move(targetPos);
                }
                else
                {
                    controller.SetCommand("Attack", controller.parameters);
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
        Station station = FindStationByName(controller.mainParameters[0]);
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
