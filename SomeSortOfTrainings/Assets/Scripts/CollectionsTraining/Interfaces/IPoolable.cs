
namespace PoolSystem.Poolable
{
    public interface IPoolable
    {
        public void OnCreatedForPool();
        public void OnAssignPool();
        public void OnEnqueuePool();
        public void OnDequeuePool();
        public void OnDeletePool();

    }

}

