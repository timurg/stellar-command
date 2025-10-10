public abstract class SpaceObjectPoolManager<T> : EntityPoolManager<T>, IPoolManager<T> where T : SpaceObject
{
    protected override void ActivateEntity(T entity)
    {
        base.ActivateEntity(entity);
        entity.SetAlive(true);
    }

    override protected void DeactivateEntity(T entity)
    {
        base.DeactivateEntity(entity);
        entity.SetAlive(false);
    }   
}