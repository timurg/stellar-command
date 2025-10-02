using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Ship : Entity
{
    [SerializeField] protected float shields = 500f; // Щиты
    [SerializeField] protected float maxShields = 500f; // Максимум щитов

    protected List<Weapon> weapons = new List<Weapon>(); // Набор оружия
    [SerializeField]  protected Entity target; // Цель
    public enum ShipState
    {
        HANGAR,    // В ангаре (refuel/repair)
        PATROL,    // Патруль вокруг Carrier
        ATTACK,    // Атака врага
        RETURN,    // Возврат к Carrier
        DAMAGED    // Повреждён, в ангаре на ремонт
    }
    private ShipState state = ShipState.HANGAR; // Приватное поле
    protected override void Awake()
    {
        base.Awake();
        // Инициализация оружия (добавь в Inspector или коде)
    }
    protected void Start()
    {
        IgnoreShipCollisions(); // Настройка игнорирования коллизий между кораблями
    }
    protected override void Update()
    {
        base.Update();
        if (!IsAlive()) return;
        if (shields < maxShields) shields += Time.deltaTime * 10f; // Пример регена щитов
        ShootAtTarget(); // Вызов общего метода стрельбы
    }
    // Обновлённый метод для движения с ротацией
    public void SetTarget(Entity newTarget)
    {
        target = newTarget;
    }
    // Общий метод стрельбы
    protected virtual void ShootAtTarget()
    {
        if (target == null) target = SelectTarget(); // Выбираем цель, если нет
        if (target != null && IsAlive())
        {
            foreach (var weapon in weapons)
            {
                if (weapon != null)
                {
                    weapon.SetTarget(target.transform.position); // Устанавливаем цель
                    weapon.ShootIfReady(); // Активируем стрельбу
                }
            }
        }
    }
    // Абстрактный метод для выбора цели
    protected abstract Entity SelectTarget();
    // Геттер и контролируемый сеттер для State
    public ShipState GetState() => state;
    public virtual void SetState(ShipState newState)
    {
        state = newState;
        // Дополнительная логика при смене состояния (можно переопределить)
        switch (newState)
        {
            case ShipState.DAMAGED:
                if (shields <= 0) SetAlive(false); // Пример: смерть при нулевых щитах
                break;
        }
    }
    // Метод для игнорирования коллизий между кораблями
    private void IgnoreShipCollisions()
    {
        Ship[] ships = FindObjectsByType<Ship>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"Found {ships.Length} ships for collision ignoring.");
        foreach (var otherShip in ships)
        {
            if (otherShip != this && otherShip.GetComponent<Collider2D>() != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherShip.GetComponent<Collider2D>(), true);
                Debug.Log($"Ignoring collision between {this.name} and {otherShip.name}");
            }
        }
    }

    public float Shields
    {
        get => shields;
        set => shields = Mathf.Clamp(value, 0, maxShields);
    }

    public float MaxShields
    {
        get => maxShields;
        set => maxShields = value;
    }

}