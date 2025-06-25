using System.Collections.Concurrent;
using HSE.Finance.Core.Interfaces;

namespace HSE.Finance.Services.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly ConcurrentDictionary<Guid, T> _storage = new();

    public void Add(T entity)
    {
        var id = (Guid)entity.GetType().GetProperty("Id")!.GetValue(entity)!;
        _storage.TryAdd(id, entity);
    }

    public void Delete(Guid id) => _storage.TryRemove(id, out _);

    public IEnumerable<T> GetAll() => _storage.Values.ToList();

    public T? GetById(Guid id) => _storage.TryGetValue(id, out var entity) ? entity : null;

    public void Update(T entity)
    {
        var id = (Guid)entity.GetType().GetProperty("Id")!.GetValue(entity)!;
        if (_storage.ContainsKey(id))
        {
            _storage[id] = entity;
        }
    }
}
