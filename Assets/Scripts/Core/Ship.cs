using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Ship : SpaceObject
{
    [SerializeField] protected float shields = 500f; // Щиты
    [SerializeField] protected float maxShields = 500f; // Максимум щитов

    [SerializeField] protected List<WeaponGun> weapons = new List<WeaponGun>(); // Набор оружия (контроллеры оружия)
    [SerializeField]  protected SpaceObject target; // Цель
    public enum ShipState
    {
        HANGAR,    // В ангаре (refuel/repair)
        PATROL,    // Патруль вокруг Carrier
        ATTACK,    // Атака врага
        RETURN,    // Возврат к Carrier
        DAMAGED    // Повреждён, в ангаре на ремонт
    }
    [SerializeField] private ShipState state = ShipState.HANGAR; // Приватное поле
    /// <summary>
    /// Минимальная дистанция атаки (самое короткодействующее оружие)
    /// </summary>
    public float MinAttackDistance
    {
        get
        {
            float min = float.MaxValue;
            bool hasAny = false;
            foreach (var w in weapons)
            {
                if (w == null) continue;
                hasAny = true;
                if (w.EffectiveRange < min) min = w.EffectiveRange;
            }
            return hasAny ? min : 0f;
        }
    }

    /// <summary>
    /// Максимальная дистанция атаки (самое дальнобойное оружие)
    /// </summary>
    public float MaxAttackDistance
    {
        get
        {
            float max = 0f;
            foreach (var w in weapons)
            {
                if (w == null) continue;
                if (w.EffectiveRange > max) max = w.EffectiveRange;
            }
            return max;
        }
    }

    /// <summary>
    /// Готовы ли все орудия атаковать цель на заданной дистанции
    /// </summary>
    public bool AllWeaponsInRange(float distance)
    {
        foreach (var w in weapons)
            if (w != null && distance > w.EffectiveRange)
                return false;
        return true;
    }

    /// <summary>
    /// Есть ли хотя бы одно орудие, способное атаковать на этой дистанции
    /// </summary>
    public bool AnyWeaponInRange(float distance)
    {
        foreach (var w in weapons)
            if (w != null && distance <= w.EffectiveRange)
                return true;
        return false;
    }

    
    protected override void Awake()
    {
        base.Awake();
        // Инициализация оружия (добавь в Inspector или коде)
    }
    protected void Start()
    {
         // Настройка игнорирования коллизий между кораблями
    }

    protected void OnEnable(){
        IgnoreShipCollisions();
    }

    protected override void Update()
    {
        base.Update();
        if (!IsAlive()) return;
        if (shields < maxShields) shields += Time.deltaTime * 10f; // Пример регена щитов
        ShootAtTarget(); // Вызов общего метода стрельбы
    }
    // Обновлённый метод для движения с ротацией
    public void SetTarget(SpaceObject newTarget)
    {
        target = newTarget;
    }
    // Общий метод стрельбы
    protected virtual void ShootAtTarget()
    {
        //var is_carrier = this is Carrier;
        //if (is_carrier) Debug.Log("Carrier ShootAtTarget called");
        if (target == null || !target.IsAlive())
        {
            //if (is_carrier) Debug.Log("Carrier SelectTarget");
            target = SelectTarget(); // Выбираем цель, если нет
        }
        else{
            //if (is_carrier) Debug.Log("Carrier Current target: " + target.name);
        }
        if (target != null && IsAlive())
        {
            foreach (var weapon in weapons)
            {
                if (weapon != null)
                {
                    //if (is_carrier) Debug.Log("Carrier ShootAtTarget weapon: " + weapon.name);
                    weapon.SetTarget(target); // Устанавливаем цель для оружия
                    weapon.ShootIfReady(); // Пытаемся выстрелить (с учётом кулдауна)
                }
            }
        }
        else{
            //if (is_carrier) Debug.Log("No target available for shooting.");
        }
    }
    // Абстрактный метод для выбора цели
    protected abstract SpaceObject SelectTarget();
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
        Ship[] ships = FindObjectsByType<Ship>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var otherShip in ships)
        {
            if (otherShip != this && otherShip.GetComponent<Collider2D>() != null)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherShip.GetComponent<Collider2D>(), true);
            }
        }
    }

    public override void TakeDamage(float damage, bool ignoreShields = false)
    {
        if (!ignoreShields && shields > 0){
            shields -= damage;
        } else{
            Health -= damage;
        }
        if (Health <= 0 && IsAlive())
        {
            SetAlive(false); // Используем метод для изменения состояния
            OnDeath();
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